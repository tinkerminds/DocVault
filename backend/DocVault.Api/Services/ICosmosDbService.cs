using DocVault.Api.Models;

namespace DocVault.Api.Services;

public interface ICosmosDbService
{
    /// <summary>
    /// Add a new document metadata record.
    /// </summary>
    Task<DocumentMetadata> AddDocumentAsync(DocumentMetadata document);

    /// <summary>
    /// Get all documents for a specific user.
    /// </summary>
    /// <param name="userId">The user's ID (partition key)</param>
    Task<IEnumerable<DocumentMetadata>> GetDocumentsAsync(string userId);

    /// <summary>
    /// Get a single document by ID and user ID.
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <param name="userId">User ID (partition key)</param>
    Task<DocumentMetadata?> GetDocumentAsync(string id, string userId);

    /// <summary>
    /// Update an existing document metadata record.
    /// </summary>
    Task<DocumentMetadata> UpdateDocumentAsync(DocumentMetadata document);

    /// <summary>
    /// Delete a document metadata record.
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <param name="userId">User ID (partition key)</param>
    Task DeleteDocumentAsync(string id, string userId);

    /// <summary>
    /// Search documents across all users by filename, tags, or excerpt content.
    /// </summary>
    /// <param name="searchTerm">The search term to match against</param>
    Task<IEnumerable<DocumentMetadata>> SearchDocumentsAsync(string searchTerm);
}
