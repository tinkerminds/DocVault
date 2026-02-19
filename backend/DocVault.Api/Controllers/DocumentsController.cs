using System.Security.Claims;
using DocVault.Api.Models;
using DocVault.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace DocVault.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IBlobStorageService _blobService;
    private readonly ICosmosDbService _cosmosService;
    private readonly ILogger<DocumentsController> _logger;
    private readonly ServiceBusClient _serviceBusClient;

    public DocumentsController(
        IBlobStorageService blobService,
        ICosmosDbService cosmosService,
        ILogger<DocumentsController> logger,
        ServiceBusClient serviceBusClient)
    {
        _blobService = blobService;
        _cosmosService = cosmosService;
        _logger = logger;
        _serviceBusClient = serviceBusClient;
    }

    /// <summary>
    /// Upload a document (multipart/form-data).
    /// POST /api/documents
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<DocumentUploadResponse>> Upload(IFormFile file, [FromForm] string? tags)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided.");

        var userId = GetUserId();

        using var stream = file.OpenReadStream();
        var blobUrl = await _blobService.UploadAsync(stream, file.FileName, file.ContentType, userId);

        var document = new DocumentMetadata
        {
            UserId = userId,
            FileName = file.FileName,
            BlobUrl = blobUrl,
            ContentType = file.ContentType,
            SizeBytes = file.Length,
            Tags = string.IsNullOrWhiteSpace(tags)
                ? new List<string>()
                : tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
            Status = "pending"
        };

        var created = await _cosmosService.AddDocumentAsync(document);

        // Send message to Service Bus queue for background processing
        var sender = _serviceBusClient.CreateSender("document-processing");

        var messageBody = new
        {
            documentId = created.Id,
            userId = created.UserId,
            blobUrl = created.BlobUrl,
            fileName = created.FileName
        };

        var message = new ServiceBusMessage(
            JsonSerializer.Serialize(messageBody));

        await sender.SendMessageAsync(message);

        _logger.LogInformation("Message sent to Service Bus for document {DocumentId}", created.Id);


        var response = MapToResponse(created);

        _logger.LogInformation("Document {DocumentId} uploaded by user {UserId}", created.Id, userId);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
    }

    /// <summary>
    /// List current user's documents.
    /// GET /api/documents
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DocumentUploadResponse>>> GetAll()
    {
        var userId = GetUserId();

        var documents = await _cosmosService.GetDocumentsAsync(userId);
        var response = documents.Select(MapToResponse);

        return Ok(response);
    }

    /// <summary>
    /// Get a single document metadata + SAS download URL.
    /// GET /api/documents/{id}
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<DocumentUploadResponse>> GetById(string id)
    {
        var userId = GetUserId();

        var document = await _cosmosService.GetDocumentAsync(id, userId);

        if (document == null || document.Status == "deleted")
            return NotFound();

        var response = MapToResponse(document);

        return Ok(response);
    }

    /// <summary>
    /// Soft-delete a document.
    /// DELETE /api/documents/{id}
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var userId = GetUserId();

        var document = await _cosmosService.GetDocumentAsync(id, userId);

        if (document == null || document.Status == "deleted")
            return NotFound();

        await _cosmosService.DeleteDocumentAsync(id, userId);

        _logger.LogInformation("Document {DocumentId} soft-deleted by user {UserId}", id, userId);

        return NoContent();
    }

    /// <summary>
    /// Extract the authenticated user's Object ID (oid) from the JWT token.
    /// </summary>
    private string GetUserId()
    {
        return User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID claim not found in token.");
    }

    private DocumentUploadResponse MapToResponse(DocumentMetadata doc)
    {
        return new DocumentUploadResponse
        {
            Id = doc.Id,
            FileName = doc.FileName,
            ContentType = doc.ContentType,
            SizeBytes = doc.SizeBytes,
            UploadedAt = doc.UploadedAt,
            Status = doc.Status,
            DownloadUrl = _blobService.GetSasDownloadUrl(doc.BlobUrl)
        };
    }
}
