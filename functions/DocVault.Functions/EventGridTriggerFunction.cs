using Azure.Messaging;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DocVault.Functions.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DocVault.Functions;

/// <summary>
/// Azure Function triggered by Event Grid when a DocumentUploaded event is published.
/// Processes the uploaded document: generates thumbnail and extracts text, then updates Cosmos DB.
/// </summary>
public class EventGridTriggerFunction
{
    private readonly ILogger<EventGridTriggerFunction> _logger;
    private readonly IThumbnailService _thumbnailService;
    private readonly ITextExtractionService _textExtractionService;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly Container _cosmosContainer;

    public EventGridTriggerFunction(
        ILogger<EventGridTriggerFunction> logger,
        IThumbnailService thumbnailService,
        ITextExtractionService textExtractionService,
        BlobServiceClient blobServiceClient,
        CosmosClient cosmosClient,
        IConfiguration config)
    {
        _logger = logger;
        _thumbnailService = thumbnailService;
        _textExtractionService = textExtractionService;
        _blobServiceClient = blobServiceClient;

        var databaseName = config.GetValue<string>("CosmosDbDatabaseName") ?? "docvault-db";
        var containerName = config.GetValue<string>("CosmosDbContainerName") ?? "documents";
        _cosmosContainer = cosmosClient.GetContainer(databaseName, containerName);
    }

    [Function(nameof(EventGridTriggerFunction))]
    public async Task Run(
        [EventGridTrigger] CloudEvent cloudEvent,
        FunctionContext context)
    {
        _logger.LogInformation(
            "EventGrid trigger fired. Event Type: {EventType}, Subject: {Subject}",
            cloudEvent.Type, cloudEvent.Subject);

        // Deserialize the event data
        var eventData = cloudEvent.Data?.ToObjectFromJson<DocumentUploadedEventData>();

        if (eventData == null)
        {
            _logger.LogWarning("Event data is null or could not be deserialized. Skipping.");
            return;
        }

        _logger.LogInformation(
            "Processing DocumentUploaded event: DocumentId={DocumentId}, UserId={UserId}, FileName={FileName}",
            eventData.DocumentId, eventData.UserId, eventData.FileName);

        try
        {
            // ── Step 1: Download the blob from storage ──────────────────────
            var blobUri = new Uri(eventData.BlobUrl);
            var blobName = GetBlobNameFromUrl(blobUri);
            var uploadsContainer = _blobServiceClient.GetBlobContainerClient("uploads");
            var blobClient = uploadsContainer.GetBlobClient(blobName);

            using var memoryStream = new MemoryStream();
            await blobClient.DownloadToAsync(memoryStream);
            memoryStream.Position = 0;

            var contentType = eventData.ContentType ?? "application/octet-stream";

            _logger.LogInformation(
                "Downloaded blob '{BlobName}' ({Size} bytes), content type: {ContentType}",
                blobName, memoryStream.Length, contentType);

            string? thumbnailUrl = null;
            string? excerpt = null;

            // ── Step 2: Generate thumbnail ──────────────────────────────────
            memoryStream.Position = 0;
            var thumbnailStream = await _thumbnailService.GenerateAsync(memoryStream, contentType);

            if (thumbnailStream != null)
            {
                var thumbnailsContainer = _blobServiceClient.GetBlobContainerClient("thumbnails");
                await thumbnailsContainer.CreateIfNotExistsAsync(PublicAccessType.None);

                var thumbnailBlobClient = thumbnailsContainer.GetBlobClient(blobName);

                await thumbnailBlobClient.UploadAsync(
                    thumbnailStream,
                    new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders { ContentType = "image/jpeg" }
                    });

                thumbnailUrl = thumbnailBlobClient.Uri.ToString();

                _logger.LogInformation(
                    "Thumbnail uploaded to 'thumbnails' container. URL: {ThumbnailUrl}", thumbnailUrl);
            }
            else
            {
                _logger.LogInformation(
                    "No thumbnail generated for '{BlobName}' (unsupported content type: {ContentType})",
                    blobName, contentType);
            }

            // ── Step 3: Extract text for searchable excerpt ─────────────────
            memoryStream.Position = 0;
            excerpt = await _textExtractionService.ExtractAsync(memoryStream, contentType);

            if (excerpt != null)
            {
                _logger.LogInformation("Extracted excerpt ({Length} chars) for '{BlobName}'", excerpt.Length, blobName);
            }

            // ── Step 4: Update Cosmos DB document ───────────────────────────
            await UpdateDocumentInCosmosDb(
                eventData.DocumentId, eventData.UserId, thumbnailUrl, excerpt, "processed");

            _logger.LogInformation(
                "Successfully processed document {DocumentId} via Event Grid trigger", eventData.DocumentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process document {DocumentId} from Event Grid event", eventData.DocumentId);

            // Mark the document as failed in Cosmos DB
            try
            {
                await UpdateDocumentInCosmosDb(
                    eventData.DocumentId, eventData.UserId, null, null, "failed");
            }
            catch (Exception updateEx)
            {
                _logger.LogError(updateEx,
                    "Failed to update Cosmos DB status to 'failed' for document {DocumentId}", eventData.DocumentId);
            }
        }
    }

    /// <summary>
    /// Extract the blob name (path within the container) from a full blob URL.
    /// Example: https://account.blob.core.windows.net/uploads/userId/guid-file.pdf → userId/guid-file.pdf
    /// </summary>
    private static string GetBlobNameFromUrl(Uri blobUri)
    {
        // The path is like /uploads/userId/guid-filename.pdf
        // We need to strip the container name prefix to get userId/guid-filename.pdf
        var path = blobUri.AbsolutePath; // /uploads/userId/guid-filename.pdf
        var segments = path.Split('/', 3); // ["", "uploads", "userId/guid-filename.pdf"]
        return segments.Length >= 3 ? Uri.UnescapeDataString(segments[2]) : Uri.UnescapeDataString(path.TrimStart('/'));
    }

    /// <summary>
    /// Update the document in Cosmos DB with thumbnail, excerpt, and status.
    /// Uses the document ID directly (unlike BlobTriggerFunction which queries by blobUrl).
    /// </summary>
    private async Task UpdateDocumentInCosmosDb(
        string documentId, string userId, string? thumbnailUrl, string? excerpt, string status)
    {
        var patchOperations = new List<PatchOperation>
        {
            PatchOperation.Set("/status", status)
        };

        if (thumbnailUrl != null)
        {
            patchOperations.Add(PatchOperation.Set("/thumbnailUrl", thumbnailUrl));
        }

        if (excerpt != null)
        {
            patchOperations.Add(PatchOperation.Set("/excerpt", excerpt));
        }

        await _cosmosContainer.PatchItemAsync<dynamic>(
            documentId,
            new PartitionKey(userId),
            patchOperations);

        _logger.LogInformation(
            "Updated Cosmos DB document {DocId}: status={Status}, thumbnailUrl={HasThumb}, excerpt={HasExcerpt}",
            documentId, status, thumbnailUrl != null, excerpt != null);
    }
}

/// <summary>
/// Data model for the DocumentUploaded event payload.
/// </summary>
public class DocumentUploadedEventData
{
    public string DocumentId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string BlobUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}
