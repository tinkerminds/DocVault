using DocVault.Api.Models;

namespace DocVault.Api.Services;

/// <summary>
/// Provides full-text search over a user's documents stored in Cosmos DB.
/// </summary>
public interface IDocumentSearchService
{
    /// <summary>
    /// Searches documents belonging to the specified user whose excerpt contains the given term.
    /// </summary>
    /// <param name="userId">The authenticated user's ID (Cosmos partition key).</param>
    /// <param name="term">The search term to match against document excerpts.</param>
    /// <returns>A collection of matching documents as <see cref="DocumentSearchDto"/>.</returns>
    Task<IEnumerable<DocumentSearchDto>> SearchAsync(string userId, string term);
}
