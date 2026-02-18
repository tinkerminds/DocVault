using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DocVault.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;

namespace DocVault.Functions;

public class BlobTriggerFunction
{
    private readonly ILogger<BlobTriggerFunction> _logger;
    private readonly IThumbnailService _thumbnailService;
    private readonly ICosmosDbService _cosmosDbService;
    private readonly BlobServiceClient _blobServiceClient;

    public BlobTriggerFunction(
        ILogger<BlobTriggerFunction> logger,
        IThumbnailService thumbnailService,
        ICosmosDbService cosmosDbService,
        BlobServiceClient blobServiceClient)
    {
        _logger = logger;
        _thumbnailService = thumbnailService;
        _cosmosDbService = cosmosDbService;
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
            // ── Step 1: Get blob properties (content type) ─────────────────
            var uploadsContainer = _blobServiceClient.GetBlobContainerClient("uploads");
            var blobClient = uploadsContainer.GetBlobClient(name);
            var properties = await blobClient.GetPropertiesAsync();
            var contentType = properties.Value.ContentType ?? "application/octet-stream";

            _logger.LogInformation("Processing blob '{Name}' with content type '{ContentType}'", name, contentType);

            // ── Step 2: Generate thumbnail ─────────────────────────────────
            string? thumbnailUrl = null;

            if (fileStream.CanSeek)
                fileStream.Position = 0;

            var thumbnailStream = await _thumbnailService.GenerateAsync(fileStream, contentType);

            if (thumbnailStream != null)
            {
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
                _logger.LogInformation("Thumbnail uploaded: {ThumbnailUrl}", thumbnailUrl);
            }
            else
            {
                _logger.LogInformation("No thumbnail for content type: {ContentType}", contentType);
            }

            // ── Step 3: Extract text excerpt ───────────────────────────────
            string? excerpt = null;

            if (fileStream.CanSeek)
                fileStream.Position = 0;

            excerpt = await ExtractTextAsync(fileStream, contentType);

            if (excerpt != null)
            {
                _logger.LogInformation("Extracted excerpt: {ExcerptLength} chars", excerpt.Length);
            }

            // ── Step 4: Update Cosmos DB document ──────────────────────────
            var blobUrl = blobClient.Uri.ToString();
            var document = await _cosmosDbService.GetDocumentByBlobUrlAsync(blobUrl);

            if (document != null)
            {
                document.ThumbnailUrl = thumbnailUrl;
                document.Excerpt = excerpt;
                document.Status = "processed";

                await _cosmosDbService.UpdateDocumentAsync(document);

                _logger.LogInformation(
                    "Cosmos DB updated: document {DocumentId} → status=processed", document.Id);
            }
            else
            {
                _logger.LogWarning(
                    "No Cosmos DB document found for blob '{Name}'. Thumbnail and excerpt generated but metadata not updated.",
                    name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process blob '{Name}'", name);

            // Try to mark document as failed in Cosmos DB
            try
            {
                var blobUrl = _blobServiceClient
                    .GetBlobContainerClient("uploads")
                    .GetBlobClient(name)
                    .Uri.ToString();

                var document = await _cosmosDbService.GetDocumentByBlobUrlAsync(blobUrl);
                if (document != null)
                {
                    document.Status = "failed";
                    await _cosmosDbService.UpdateDocumentAsync(document);
                    _logger.LogInformation("Marked document {DocumentId} as failed", document.Id);
                }
            }
            catch (Exception innerEx)
            {
                _logger.LogError(innerEx, "Failed to mark document as failed in Cosmos DB");
            }
        }
    }

    /// <summary>
    /// Extract text from the blob stream based on content type.
    /// Returns first 500 characters as excerpt, or null if unsupported.
    /// </summary>
    private async Task<string?> ExtractTextAsync(Stream stream, string contentType)
    {
        try
        {
            var lower = contentType.ToLowerInvariant();

            if (lower == "application/pdf")
            {
                return ExtractPdfText(stream);
            }

            if (lower.StartsWith("text/"))
            {
                using var reader = new StreamReader(stream, leaveOpen: true);
                var text = await reader.ReadToEndAsync();
                return TruncateExcerpt(text);
            }

            _logger.LogInformation("Text extraction not supported for: {ContentType}", contentType);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract text from {ContentType}", contentType);
            return null;
        }
    }

    /// <summary>
    /// Extract all text from a PDF using PdfPig, return first 500 chars.
    /// </summary>
    private string? ExtractPdfText(Stream stream)
    {
        using var pdf = PdfDocument.Open(stream);
        var textBuilder = new System.Text.StringBuilder();

        foreach (var page in pdf.GetPages())
        {
            textBuilder.Append(page.Text);
            textBuilder.Append(' ');

            // Stop early if we already have enough text
            if (textBuilder.Length >= 500)
                break;
        }

        var text = textBuilder.ToString().Trim();
        return string.IsNullOrEmpty(text) ? null : TruncateExcerpt(text);
    }

    /// <summary>
    /// Truncate text to 500 characters.
    /// </summary>
    private static string TruncateExcerpt(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        return text.Length <= 500
            ? text.Trim()
            : text[..500].Trim() + "...";
    }
}
