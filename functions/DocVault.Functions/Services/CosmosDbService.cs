using DocVault.Functions.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DocVault.Functions.Services;

public class CosmosDbService : ICosmosDbService
{
    private readonly Container _container;
    private readonly ILogger<CosmosDbService> _logger;

    public CosmosDbService(CosmosClient cosmosClient, IConfiguration configuration, ILogger<CosmosDbService> logger)
    {
        _logger = logger;

        var databaseName = configuration["CosmosDb__DatabaseName"] ?? "docvault-db";
        var containerName = configuration["CosmosDb__ContainerName"] ?? "documents";

        _container = cosmosClient.GetContainer(databaseName, containerName);
    }

    public async Task<DocumentMetadata?> GetDocumentByBlobUrlAsync(string blobUrl)
    {
        // Cross-partition query — find document matching the blob URL
        var query = new QueryDefinition("SELECT * FROM c WHERE c.blobUrl = @blobUrl")
            .WithParameter("@blobUrl", blobUrl);

        using var iterator = _container.GetItemQueryIterator<DocumentMetadata>(
            query,
            requestOptions: new QueryRequestOptions { MaxItemCount = 1 });

        while (iterator.HasMoreResults)
        {
            var page = await iterator.ReadNextAsync();
            var doc = page.FirstOrDefault();

            if (doc != null)
            {
                _logger.LogInformation("Found Cosmos document {DocumentId} for blobUrl {BlobUrl}", doc.Id, blobUrl);
                return doc;
            }
        }

        _logger.LogWarning("No Cosmos document found for blobUrl {BlobUrl}", blobUrl);
        return null;
    }

    public async Task<DocumentMetadata> UpdateDocumentAsync(DocumentMetadata document)
    {
        var response = await _container.ReplaceItemAsync(
            document,
            document.Id,
            new PartitionKey(document.UserId));

        _logger.LogInformation(
            "Updated Cosmos document {DocumentId}: status={Status}, excerpt={ExcerptLength} chars",
            document.Id,
            document.Status,
            document.Excerpt?.Length ?? 0);

        return response.Resource;
    }
}
