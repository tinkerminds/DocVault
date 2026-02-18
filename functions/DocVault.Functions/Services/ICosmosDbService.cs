using DocVault.Functions.Models;

namespace DocVault.Functions.Services;

public interface ICosmosDbService
{
    /// <summary>
    /// Find a document by its blob URL (cross-partition query).
    /// </summary>
    Task<DocumentMetadata?> GetDocumentByBlobUrlAsync(string blobUrl);

    /// <summary>
    /// Update an existing document (excerpt, thumbnailUrl, status).
    /// </summary>
    Task<DocumentMetadata> UpdateDocumentAsync(DocumentMetadata document);
}
