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

    public async Task<IEnumerable<DocumentMetadata>> SearchDocumentsAsync(string searchTerm)
    {
        var queryText = @"
            SELECT * FROM c 
            WHERE c.status != 'deleted' 
            AND (
                CONTAINS(LOWER(c.fileName), LOWER(@term)) 
                OR CONTAINS(LOWER(c.excerpt), LOWER(@term))
                OR ARRAY_CONTAINS(c.tags, @term)
            )";

        var query = new QueryDefinition(queryText)
            .WithParameter("@term", searchTerm);

        // Cross-partition query — searches across all users
        var iterator = _container.GetItemQueryIterator<DocumentMetadata>(query);

        var results = new List<DocumentMetadata>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }

        _logger.LogInformation("Search for '{SearchTerm}' returned {Count} results", searchTerm, results.Count);
        return results;
    }

    /// <summary>
    /// Counts all non-deleted documents across all partitions (all users).
    /// Uses a cross-partition aggregate query for efficiency — no need to
    /// materialise every document.
    /// </summary>
    public async Task<int> GetTotalDocumentCountAsync()
    {
        var query = new QueryDefinition(
            "SELECT VALUE COUNT(1) FROM c WHERE c.status != 'deleted'");

        var iterator = _container.GetItemQueryIterator<int>(
            query,
            requestOptions: new QueryRequestOptions { MaxItemCount = 1 });

        int total = 0;
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            foreach (var value in response)
                total += value;
        }

        _logger.LogInformation("Total document count (cross-partition): {Count}", total);
        return total;
    }

    /// <summary>
    /// Counts distinct userId values across all non-deleted documents.
    /// This is a cross-partition query that runs against all partitions.
    /// </summary>
    public async Task<int> GetDistinctUserCountAsync()
    {
        // Cosmos DB does not support COUNT(DISTINCT ...) in a single query,
        // so we fetch all distinct userIds and count them in memory.
        // This is efficient because we only SELECT the userId field.
        var query = new QueryDefinition(
            "SELECT DISTINCT VALUE c.userId FROM c WHERE c.status != 'deleted'");

        var iterator = _container.GetItemQueryIterator<string>(query);

        var userIds = new HashSet<string>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            foreach (var uid in response)
                if (!string.IsNullOrEmpty(uid))
                    userIds.Add(uid);
        }

        _logger.LogInformation("Distinct user count: {Count}", userIds.Count);
        return userIds.Count;
    }
}

