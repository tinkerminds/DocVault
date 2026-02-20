using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DocVault.Functions.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DocVault.Functions;

public class DocumentProcessorFunction
{
    private readonly ILogger<DocumentProcessorFunction> _logger;
    private readonly IThumbnailService _thumbnailService;
    private readonly ITextExtractionService _textExtractionService;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly Container _cosmosContainer;

    public DocumentProcessorFunction(
        ILogger<DocumentProcessorFunction> logger,
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

    [Function("DocumentProcessorFunction")]
    public async Task Run(
        [ServiceBusTrigger("document-processing", Connection = "ServiceBusConnection")]
        string message)
    {
        try
        {
            _logger.LogInformation("Service Bus trigger function received message: {Message}", message);

            // Parse the JSON message from backend
            dynamic messageData;
            try
            {
                messageData = JsonConvert.DeserializeObject<dynamic>(message);
            }
            catch (JsonException)
            {
                _logger.LogWarning("Failed to parse JSON message. Raw message: {Message}", message);
                return;
            }

            var userId = (string?)messageData?.userId;
            var blobUrl = (string?)messageData?.blobUrl;
            var documentId = (string?)messageData?.documentId;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(blobUrl))
            {
                _logger.LogWarning("Invalid message format. Missing userId or blobUrl. Message: {Message}", message);
                return;
            }

            // Extract blobName from blobUrl: https://account.blob.core.windows.net/uploads/{userId}/{fileName}
            var uri = new Uri(blobUrl);
            var blobName = uri.AbsolutePath.TrimStart('/').Replace("uploads/", "", StringComparison.OrdinalIgnoreCase);

            _logger.LogInformation("Processing document: DocumentId={DocumentId}, UserId={UserId}, BlobName={BlobName}",
                documentId, userId, blobName);

            // ── Read blob from storage ────────────────────────────────────
            var uploadsContainer = _blobServiceClient.GetBlobContainerClient("uploads");
            var blobClient = uploadsContainer.GetBlobClient(blobName);

            // Check if blob exists
            if (!await blobClient.ExistsAsync())
            {
                _logger.LogWarning("Blob not found: {BlobName}", blobName);
                return;
            }

            var properties = await blobClient.GetPropertiesAsync();
            var contentType = properties.Value.ContentType ?? "application/octet-stream";

            _logger.LogInformation("Blob found with content type: {ContentType}", contentType);

            // ── Buffer blob into memory ───────────────────────────────────
            using var memoryStream = new MemoryStream();
            var download = await blobClient.DownloadAsync();
            await download.Value.Content.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            string? thumbnailUrl = null;
            string? excerpt = null;

            // ── Generate thumbnail ────────────────────────────────────────
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
                _logger.LogInformation("Thumbnail uploaded: {ThumbnailUrl}", thumbnailUrl);
            }

            // ── Extract text for searchable excerpt ────────────────────────
            memoryStream.Position = 0;
            excerpt = await _textExtractionService.ExtractAsync(memoryStream, contentType);
            if (excerpt != null)
            {
                _logger.LogInformation("Extracted excerpt ({Length} chars)", excerpt.Length);
            }

            // ── Update Cosmos DB ──────────────────────────────────────────
            await UpdateDocumentInCosmosDb(userId, blobName, thumbnailUrl, excerpt, "processed");

            _logger.LogInformation("Successfully processed document: {DocumentId}", documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Service Bus message");
            throw;
        }
    }

    private async Task UpdateDocumentInCosmosDb(
        string userId, string blobName, string? thumbnailUrl, string? excerpt, string status)
    {
        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.userId = @userId AND CONTAINS(c.blobUrl, @blobName)")
                .WithParameter("@userId", userId)
                .WithParameter("@blobName", blobName);

            var iterator = _cosmosContainer.GetItemQueryIterator<dynamic>(
                query,
                requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(userId) });

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                foreach (var doc in response)
                {
                    doc.thumbnailUrl = thumbnailUrl;
                    doc.status = status;
                    if (excerpt != null)
                        doc.excerpt = excerpt;

                    string docId = doc.id;
                    await _cosmosContainer.ReplaceItemAsync<dynamic>(
                        doc, docId, new PartitionKey(userId));

                    _logger.LogInformation(
                        "Updated Cosmos DB document {DocId}: status={Status}",
                        docId, status);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Cosmos DB for blob: {BlobName}", blobName);
            throw;
        }
    }
}
