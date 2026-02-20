using DocVault.Api.Models;

namespace DocVault.Api.Services;

/// <summary>
/// No-op implementation of IEventGridService used when EventGrid is not configured
/// (e.g. local development without credentials). Logs a warning instead of throwing.
/// </summary>
public class NoOpEventGridService : IEventGridService
{
    private readonly ILogger<NoOpEventGridService> _logger;

    public NoOpEventGridService(ILogger<NoOpEventGridService> logger)
    {
        _logger = logger;
    }

    public Task PublishDocumentUploadedEventAsync(DocumentMetadata document)
    {
        _logger.LogWarning("EventGrid is not configured. Skipping event publish for document {DocumentId}.", document.Id);
        return Task.CompletedTask;
    }
}
