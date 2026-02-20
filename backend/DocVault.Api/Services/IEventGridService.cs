using DocVault.Api.Models;

namespace DocVault.Api.Services;

public interface IEventGridService
{
    /// <summary>
    /// Publish a DocumentUploaded event to Azure Event Grid.
    /// </summary>
    /// <param name="document">The document metadata to include in the event.</param>
    Task PublishDocumentUploadedEventAsync(DocumentMetadata document);
}
