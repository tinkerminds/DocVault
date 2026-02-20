using Azure;
using Azure.Messaging;
using Azure.Messaging.EventGrid;
using DocVault.Api.Models;
using System.Text.Json;

namespace DocVault.Api.Services;

public class EventGridService : IEventGridService
{
    private readonly EventGridPublisherClient _client;
    private readonly ILogger<EventGridService> _logger;

    public EventGridService(IConfiguration configuration, ILogger<EventGridService> logger)
    {
        _logger = logger;

        var topicEndpoint = configuration["EventGrid:TopicEndpoint"]
            ?? throw new InvalidOperationException("EventGrid:TopicEndpoint is not configured.");
        var topicKey = configuration["EventGrid:TopicKey"]
            ?? throw new InvalidOperationException("EventGrid:TopicKey is not configured.");

        _client = new EventGridPublisherClient(
            new Uri(topicEndpoint),
            new AzureKeyCredential(topicKey));
    }

    public async Task PublishDocumentUploadedEventAsync(DocumentMetadata document)
    {
        var eventData = new
        {
            documentId = document.Id,
            userId = document.UserId,
            blobUrl = document.BlobUrl,
            fileName = document.FileName,
            contentType = document.ContentType
        };

        var cloudEvent = new CloudEvent(
            source: "/docvault/api/documents",
            type: "DocVault.Document.Uploaded",
            jsonSerializableData: eventData);

        await _client.SendEventAsync(cloudEvent);

        _logger.LogInformation(
            "Published DocumentUploaded event for document {DocumentId}, user {UserId}",
            document.Id, document.UserId);
    }
}
