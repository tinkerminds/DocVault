using DocVault.Api.Models;
using Microsoft.Azure.Cosmos;

namespace DocVault.Api.Services;

/// <summary>
/// Implements document search using Cosmos DB parameterized SQL queries.
/// Queries are scoped to the user's partition key for efficiency and security.
/// </summary>
public class DocumentSearchService : IDocumentSearchService
{
    private readonly Container _container;
    private readonly ILogger<DocumentSearchService> _logger;

    public DocumentSearchService(
        CosmosClient cosmosClient,
        IConfiguration configuration,
        ILogger<DocumentSearchService> logger)
    {
        _logger = logger;

        var databaseName = configuration["CosmosDatabaseName"]
            ?? configuration["CosmosDb:DatabaseName"]
            ?? "docvault-db";

        var containerName = configuration["CosmosContainerName"]
            ?? configuration["CosmosDb:ContainerName"]
            ?? "documents";

        _container = cosmosClient.GetContainer(databaseName, containerName);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DocumentSearchDto>> SearchAsync(string userId, string term)
    {
        _logger.LogInformation(
            "Document search initiated. UserId={UserId}, TermLength={TermLength}",
            userId, term.Length);

        // Parameterized query — no string concatenation, safe against injection
        var queryDefinition = new QueryDefinition(
            @"SELECT c.id, c.fileName, c.thumbnailUrl, c.excerpt, c.uploadedAt
              FROM c
              WHERE c.userId = @userId
                AND c.status != 'deleted'
                AND CONTAINS(c.excerpt, @term, true)")
            .WithParameter("@userId", userId)
            .WithParameter("@term", term);

        var requestOptions = new QueryRequestOptions
        {
            PartitionKey = new PartitionKey(userId)
        };

        var results = new List<DocumentSearchDto>();

        try
        {
            using var iterator = _container.GetItemQueryIterator<DocumentSearchDto>(
                queryDefinition,
                requestOptions: requestOptions);

            while (iterator.HasMoreResults)
            {
                var page = await iterator.ReadNextAsync();
                results.AddRange(page);
            }

            _logger.LogInformation(
                "Document search completed. UserId={UserId}, ResultCount={Count}",
                userId, results.Count);
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex,
                "Cosmos DB error during document search. UserId={UserId}, StatusCode={StatusCode}",
                userId, ex.StatusCode);
            throw;
        }

        return results;
    }
}
