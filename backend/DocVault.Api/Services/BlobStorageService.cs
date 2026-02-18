using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace DocVault.Api.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;
    private readonly BlobServiceClient _serviceClient;
    private readonly ILogger<BlobStorageService> _logger;

    public BlobStorageService(BlobServiceClient serviceClient, IConfiguration config, ILogger<BlobStorageService> logger)
    {
        _serviceClient = serviceClient;
        _logger = logger;

        var containerName = config.GetValue<string>("AzureStorage:ContainerName") ?? "uploads";
        _containerClient = serviceClient.GetBlobContainerClient(containerName);
    }

    public async Task<string> UploadAsync(Stream stream, string fileName, string contentType, string userId)
    {
        // Create blob path: {userId}/{guid}-{fileName}
        var blobName = $"{userId}/{Guid.NewGuid()}-{fileName}";
        var blobClient = _containerClient.GetBlobClient(blobName);

        var headers = new BlobHttpHeaders { ContentType = contentType };

        await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = headers });

        _logger.LogInformation("Uploaded blob {BlobName} for user {UserId}", blobName, userId);

        return blobClient.Uri.ToString();
    }

    public string GetSasDownloadUrl(string blobUrl)
    {
        var blobUri = new Uri(blobUrl);
        var blobClient = new BlobClient(blobUri, _serviceClient.GetProperties().Value != null
            ? null
            : null);

        // Parse container and blob name from the URL
        var segments = blobUri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2)
        {
            _logger.LogWarning("Invalid blob URL format: {BlobUrl}", blobUrl);
            return blobUrl;
        }

        var containerName = segments[0];
        var blobName = string.Join("/", segments.Skip(1));

        var containerClient = _serviceClient.GetBlobContainerClient(containerName);
        var targetBlobClient = containerClient.GetBlobClient(blobName);

        // Generate SAS token valid for 1 hour
        if (targetBlobClient.CanGenerateSasUri)
        {
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasUri = targetBlobClient.GenerateSasUri(sasBuilder);
            return sasUri.ToString();
        }

        _logger.LogWarning("Cannot generate SAS URI for blob {BlobUrl}. Returning raw URL.", blobUrl);
        return blobUrl;
    }

    public async Task DeleteAsync(string blobUrl)
    {
        var blobUri = new Uri(blobUrl);
        var segments = blobUri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2) return;

        var containerName = segments[0];
        var blobName = string.Join("/", segments.Skip(1));

        var containerClient = _serviceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);

        _logger.LogInformation("Deleted blob {BlobName} from container {ContainerName}", blobName, containerName);
    }
}
