using DocVault.Api.Models;

namespace DocVault.Api.Services;

public interface ICosmosDbService
{
    /// <summary>Add a new document metadata record.</summary>
    Task<DocumentMetadata> AddDocumentAsync(DocumentMetadata document);

    /// <summary>Get all documents for a specific user.</summary>
    Task<IEnumerable<DocumentMetadata>> GetDocumentsAsync(string userId);

    /// <summary>Get a single document by ID and user ID.</summary>
    Task<DocumentMetadata?> GetDocumentAsync(string id, string userId);

    /// <summary>Update an existing document metadata record.</summary>
    Task<DocumentMetadata> UpdateDocumentAsync(DocumentMetadata document);

    /// <summary>Soft-delete a document metadata record.</summary>
    Task DeleteDocumentAsync(string id, string userId);

    /// <summary>Search documents across all users by filename, tags, or excerpt content.</summary>
    Task<IEnumerable<DocumentMetadata>> SearchDocumentsAsync(string searchTerm);

    /// <summary>
    /// Returns the total count of non-deleted documents across ALL users.
    /// Used by the analytics dashboard to show platform-wide upload totals.
    /// </summary>
    Task<int> GetTotalDocumentCountAsync();

    /// <summary>
    /// Returns the count of distinct user IDs that have at least one non-deleted document.
    /// Used by the analytics dashboard as a proxy for active users.
    /// </summary>
    Task<int> GetDistinctUserCountAsync();
}

