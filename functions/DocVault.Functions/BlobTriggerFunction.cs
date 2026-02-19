using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DocVault.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DocVault.Functions;

public class BlobTriggerFunction
{
    private readonly ILogger<BlobTriggerFunction> _logger;
    private readonly IThumbnailService _thumbnailService;
    private readonly BlobServiceClient _blobServiceClient;

    public BlobTriggerFunction(
        ILogger<BlobTriggerFunction> logger,
        IThumbnailService thumbnailService,
        BlobServiceClient blobServiceClient)
    {
        _logger = logger;
        _thumbnailService = thumbnailService;
        _blobServiceClient = blobServiceClient;
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

        try
        {
            // ── Step 1: Determine content type from blob metadata ──────────
            var uploadsContainer = _blobServiceClient.GetBlobContainerClient("uploads");
            var blobClient = uploadsContainer.GetBlobClient(name);
            var properties = await blobClient.GetPropertiesAsync();
            var contentType = properties.Value.ContentType ?? "application/octet-stream";

            _logger.LogInformation("Processing blob '{Name}' with content type '{ContentType}'", name, contentType);

            // ── Step 2: Generate thumbnail ─────────────────────────────────
            // Reset stream position so ThumbnailService reads from the beginning
            if (fileStream.CanSeek)
                fileStream.Position = 0;

            var thumbnailStream = await _thumbnailService.GenerateAsync(fileStream, contentType);

            if (thumbnailStream != null)
            {
                // ── Step 3: Upload thumbnail to 'thumbnails' container ─────
                var thumbnailsContainer = _blobServiceClient.GetBlobContainerClient("thumbnails");

                // Create the container if it doesn't exist yet
                await thumbnailsContainer.CreateIfNotExistsAsync(PublicAccessType.None);

                // Thumbnail blob name mirrors the original: e.g. userId/guid-file.jpg → userId/guid-file.jpg
                var thumbnailBlobName = name;
                var thumbnailBlobClient = thumbnailsContainer.GetBlobClient(thumbnailBlobName);

                await thumbnailBlobClient.UploadAsync(
                    thumbnailStream,
                    new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders { ContentType = "image/jpeg" }
                    });

                var thumbnailUrl = thumbnailBlobClient.Uri.ToString();

                _logger.LogInformation(
                    "Thumbnail uploaded to 'thumbnails' container. URL: {ThumbnailUrl}", thumbnailUrl);

                // TODO (next step): Update Cosmos DB document with thumbnailUrl + status = "processed"
            }
            else
            {
                _logger.LogInformation(
                    "No thumbnail generated for '{Name}' (unsupported content type: {ContentType})",
                    name, contentType);

                // TODO (next step): Update Cosmos DB document with status = "processed" (no thumbnail)
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process blob '{Name}'", name);

            // TODO (next step): Update Cosmos DB document with status = "failed"
        }
    }
}
