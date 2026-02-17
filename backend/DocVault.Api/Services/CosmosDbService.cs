using DocVault.Api.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace DocVault.Api.Services;

public class CosmosDbService : ICosmosDbService
{
    private readonly Container _container;
    private readonly ILogger<CosmosDbService> _logger;

    public CosmosDbService(CosmosClient cosmosClient, IConfiguration config, ILogger<CosmosDbService> logger)
    {
        _logger = logger;

        var databaseName = config.GetValue<string>("CosmosDb:DatabaseName") ?? "docvault-db";
        var containerName = config.GetValue<string>("CosmosDb:ContainerName") ?? "documents";

        _container = cosmosClient.GetContainer(databaseName, containerName);
    }

    public async Task<DocumentMetadata> AddDocumentAsync(DocumentMetadata document)
    {
        var response = await _container.CreateItemAsync(document, new PartitionKey(document.UserId));
        _logger.LogInformation("Created document {DocumentId} for user {UserId}", document.Id, document.UserId);
        return response.Resource;
    }

    public async Task<IEnumerable<DocumentMetadata>> GetDocumentsAsync(string userId)
    {
        var query = _container.GetItemLinqQueryable<DocumentMetadata>(
            requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(userId)
            })
            .Where(d => d.Status != "deleted")
            .ToFeedIterator();

        var results = new List<DocumentMetadata>();

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response);
        }

        _logger.LogInformation("Retrieved {Count} documents for user {UserId}", results.Count, userId);
        return results;
    }

    public async Task<DocumentMetadata?> GetDocumentAsync(string id, string userId)
    {
        try
        {
            var response = await _container.ReadItemAsync<DocumentMetadata>(id, new PartitionKey(userId));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Document {DocumentId} not found for user {UserId}", id, userId);
            return null;
        }
    }

    public async Task<DocumentMetadata> UpdateDocumentAsync(DocumentMetadata document)
    {
        var response = await _container.ReplaceItemAsync(document, document.Id, new PartitionKey(document.UserId));
        _logger.LogInformation("Updated document {DocumentId} for user {UserId}", document.Id, document.UserId);
        return response.Resource;
    }

    public async Task DeleteDocumentAsync(string id, string userId)
    {
        // Soft-delete: update status to "deleted"
        var document = await GetDocumentAsync(id, userId);
        if (document != null)
        {
            document.Status = "deleted";
            await _container.ReplaceItemAsync(document, id, new PartitionKey(userId));
            _logger.LogInformation("Soft-deleted document {DocumentId} for user {UserId}", id, userId);
        }
    }
}
