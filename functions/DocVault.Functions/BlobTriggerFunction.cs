using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DocVault.Functions.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DocVault.Functions;

public class BlobTriggerFunction
{
    private readonly ILogger<BlobTriggerFunction> _logger;
    private readonly IThumbnailService _thumbnailService;
    private readonly ITextExtractionService _textExtractionService;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly Container _cosmosContainer;

    public BlobTriggerFunction(
        ILogger<BlobTriggerFunction> logger,
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

    [Function(nameof(BlobTriggerFunction))]
    public async Task Run(
        [BlobTrigger("uploads/{name}", Connection = "AzureWebJobsStorage")] Stream fileStream,
        string name,
        FunctionContext context)
    {
        _logger.LogInformation(
            "Blob trigger fired. File: {FileName}, Size: {FileSize} bytes",
            name,
            fileStream.Length);

        // Parse userId from blob path: {userId}/{guid}-{fileName}
        var segments = name.Split('/', 2);
        if (segments.Length < 2)
        {
            _logger.LogWarning("Unexpected blob name format: {Name}. Expected userId/guid-fileName", name);
            return;
        }

        var userId = segments[0];
        var blobFileName = segments[1]; // e.g. "guid-originalname.pdf"

        try
        {
            // ── Buffer the blob stream into memory for reusability ──────────
            // Blob trigger streams are non-seekable network streams.
            // PdfPig disposes the stream after use, so we must buffer it first.
            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            // ── Step 1: Determine content type from blob metadata ──────────
            var uploadsContainer = _blobServiceClient.GetBlobContainerClient("uploads");
            var blobClient = uploadsContainer.GetBlobClient(name);
            var properties = await blobClient.GetPropertiesAsync();
            var contentType = properties.Value.ContentType ?? "application/octet-stream";

            _logger.LogInformation("Processing blob '{Name}' with content type '{ContentType}'", name, contentType);

            string? thumbnailUrl = null;
            string? excerpt = null;

            // ── Step 2: Generate thumbnail ─────────────────────────────────
            memoryStream.Position = 0;
            var thumbnailStream = await _thumbnailService.GenerateAsync(memoryStream, contentType);

            if (thumbnailStream != null)
            {
                // Upload thumbnail to 'thumbnails' container
                var thumbnailsContainer = _blobServiceClient.GetBlobContainerClient("thumbnails");
                await thumbnailsContainer.CreateIfNotExistsAsync(PublicAccessType.None);

                var thumbnailBlobClient = thumbnailsContainer.GetBlobClient(name);

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
                    "No thumbnail generated for '{Name}' (unsupported content type: {ContentType})",
                    name, contentType);
            }

            // ── Step 3: Extract text for searchable excerpt ────────────────
            memoryStream.Position = 0;
            excerpt = await _textExtractionService.ExtractAsync(memoryStream, contentType);

            if (excerpt != null)
            {
                _logger.LogInformation("Extracted excerpt ({Length} chars) for '{Name}'", excerpt.Length, name);
            }

            // ── Step 4: Update Cosmos DB document ──────────────────────────
            await UpdateDocumentInCosmosDb(userId, name, thumbnailUrl, excerpt, "processed");

            _logger.LogInformation("Successfully processed blob '{Name}' for user '{UserId}'", name, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process blob '{Name}'", name);

            // Mark the document as failed in Cosmos DB
            try
            {
                await UpdateDocumentInCosmosDb(userId, name, null, null, "failed");
            }
            catch (Exception updateEx)
            {
                _logger.LogError(updateEx, "Failed to update Cosmos DB status to 'failed' for blob '{Name}'", name);
            }
        }
    }

    /// <summary>
    /// Find the matching document in Cosmos DB by blobUrl and update its metadata.
    /// </summary>
    private async Task UpdateDocumentInCosmosDb(
        string userId, string blobName, string? thumbnailUrl, string? excerpt, string status)
    {
        // Query for the document matching this blob URL pattern
        // The blobUrl stored in Cosmos DB ends with the blob name
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
                // Update the document fields
                doc.thumbnailUrl = thumbnailUrl;
                doc.status = status;

                if (excerpt != null)
                {
                    doc.excerpt = excerpt;
                }

                string docId = doc.id;
                await _cosmosContainer.ReplaceItemAsync<dynamic>(
                    doc, docId, new PartitionKey(userId));

                _logger.LogInformation(
                    "Updated Cosmos DB document {DocId}: status={Status}, thumbnailUrl={HasThumb}, excerpt={HasExcerpt}",
                    docId, status, thumbnailUrl != null, excerpt != null);
            }
        }
    }
}
