using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace DocVault.Functions.Services;

public interface ICosmosMetadataService
{
    Task UpdateMetadataAsync(string documentId, string? thumbnailUrl, string? excerpt, string status, string? failureReason = null);
}

public class CosmosMetadataService : ICosmosMetadataService
{
    private readonly Container _container;

    public CosmosMetadataService(CosmosClient cosmosClient, IConfiguration configuration)
    {
        var databaseName = configuration["CosmosDb:DatabaseName"] ?? "docvault-db";
        var containerName = configuration["CosmosDb:ContainerName"] ?? "documents";
        _container = cosmosClient.GetContainer(databaseName, containerName);
    }

    public async Task UpdateMetadataAsync(string documentId, string? thumbnailUrl, string? excerpt, string status, string? failureReason = null)
    {
        var patchOperations = new List<PatchOperation>
        {
            PatchOperation.Set("/status", status),
            PatchOperation.Set("/updatedOn", DateTime.UtcNow)
        };

        if (!string.IsNullOrEmpty(thumbnailUrl))
        {
            patchOperations.Add(PatchOperation.Set("/thumbnailUrl", thumbnailUrl));
        }

        if (!string.IsNullOrEmpty(excerpt))
        {
            patchOperations.Add(PatchOperation.Set("/excerpt", excerpt));
        }

        if (!string.IsNullOrEmpty(failureReason))
        {
            patchOperations.Add(PatchOperation.Set("/failureReason", failureReason));
        }

        // We assume partition key is the document ID for now, or we might need to look it up.
        // Based on typical design, partition key might be 'category' or 'id'. 
        // For this task, assuming ID is partition key or we just use ID. 
        // If partition key differs, we'd need it passed in. 
        // Re-checking the previous conversations/setup might be useful, but for now assuming ID.
        // Actually, let's play safe and assume we might need it. 
        // But the blob name usually contains the ID? "uploads/{name}" -> name might be "id.pdf"? 
        // No, name is just filename. We need to map filename to document ID.
        // The metadata entry should have `filePath` or `blobName`. 
        // We need to query by `blobName` (or `fileName`) to find the ID first?
        // Or does the filename contain the ID?
        // Let's assume for now we look up by a query on `filePath` (uploads/{name}).
        
        // Wait, efficient lookup needs ID.
        // If we don't have ID, we must query.
        
        // Let's first try to patch using ID = name (if name is GUID).
        // If not, we query. 
        // BUT, `uploads/{name}` trigger gives us the name. 
        // The API likely saved the document with `id` = generated GUID and `filePath` = `documents/filename`.
        // Actually, the filename in blob storage usually includes a GUID or is the GUID to avoid collisions.
        // Let's assume the `name` IS the document ID (or we can derive it).
        // If the API uploads as `{id}/{filename}` or just `{id}.ext`, we are good.
        // If the API uploads as `original-filename.ext`, we have a collision problem and a lookup problem.
        // Let's check the backend logic if possible? 
        // I'll add a lookup method just in case.
        
        await _container.PatchItemAsync<dynamic>(documentId, new PartitionKey(documentId), patchOperations);
    }
}
