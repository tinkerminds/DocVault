using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DocVault.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DocVault.Functions;

public class BlobTriggerFunction
{
    private readonly ILogger<BlobTriggerFunction> _logger;
    private readonly IImageProcessingService _imageService;
    private readonly ITextExtractionService _textService;
    private readonly ICosmosMetadataService _cosmosService;
    private readonly BlobServiceClient _blobServiceClient;

    public BlobTriggerFunction(
        ILogger<BlobTriggerFunction> logger,
        IImageProcessingService imageService,
        ITextExtractionService textService,
        ICosmosMetadataService cosmosService,
        BlobServiceClient blobServiceClient)
    {
        _logger = logger;
        _imageService = imageService;
        _textService = textService;
        _cosmosService = cosmosService;
        _blobServiceClient = blobServiceClient;
    }

    [Function(nameof(BlobTriggerFunction))]
    public async Task Run(
        [BlobTrigger("uploads/{name}", Connection = "AzureWebJobsStorage")] Stream fileStream,
        string name,
        FunctionContext context)
    {
        _logger.LogInformation("Blob trigger fired for file: {FileName}", name);

        // Assume Document ID is the filename without extension (e.g. "guid.pdf" -> "guid")
        var documentId = Path.GetFileNameWithoutExtension(name);
        string? thumbnailUrl = null;
        string? excerpt = null;
        string status = "processed";
        string? failureReason = null;

        try
        {
            // 1. Generate Thumbnail (if image)
            if (_imageService.IsImage(name))
            {
                _logger.LogInformation("Generating thumbnail for {FileName}", name);
                try
                {
                    using var thumbnailStream = _imageService.ResizeImage(fileStream, 200, 200);
                    
                    var containerClient = _blobServiceClient.GetBlobContainerClient("thumbnails");
                    await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
                    
                    var thumbnailBlobName = $"{documentId}-thumb.jpg";
                    var blobClient = containerClient.GetBlobClient(thumbnailBlobName);
                    
                    // Upload
                    thumbnailStream.Position = 0;
                    await blobClient.UploadAsync(thumbnailStream, overwrite: true);
                    thumbnailUrl = blobClient.Uri.ToString();
                    
                    _logger.LogInformation("Thumbnail uploaded to {Url}", thumbnailUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating thumbnail for {FileName}", name);
                    // Non-fatal, proceed
                }
            }

            // 2. Extract Text
            _logger.LogInformation("Extracting text from {FileName}", name);
            try 
            {
                // Reset stream position for extraction logic if it wasn't already handled inside service
                // But TextExtractionService handles seeking.
                excerpt = await _textService.ExtractTextAsync(fileStream, name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from {FileName}", name);
                // Non-fatal
            }

            // 3. Update Metadata
            _logger.LogInformation("Updating metadata for document {DocumentId}", documentId);
            await _cosmosService.UpdateMetadataAsync(documentId, thumbnailUrl, excerpt, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error processing blob {FileName}", name);
            status = "failed";
            failureReason = ex.Message;
            
            // Attempt to update status to failed
            try
            {
                await _cosmosService.UpdateMetadataAsync(documentId, null, null, status, failureReason);
            }
            catch (Exception dbEx)
            {
                _logger.LogError(dbEx, "Failed to update error status in Cosmos DB for {DocumentId}", documentId);
            }
            
            throw; // Rethrow to ensure function runtime knows it failed (and might retry)
        }
    }
}
