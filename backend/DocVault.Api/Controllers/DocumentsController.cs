using DocVault.Api.Models;
using DocVault.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocVault.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IBlobStorageService _blobService;
    private readonly ICosmosDbService _cosmosService;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(
        IBlobStorageService blobService,
        ICosmosDbService cosmosService,
        ILogger<DocumentsController> logger)
    {
        _blobService = blobService;
        _cosmosService = cosmosService;
        _logger = logger;
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

        // TODO: Replace with authenticated user ID from Entra ID (Day 2)
        var userId = "demo-user";

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
        // TODO: Replace with authenticated user ID from Entra ID (Day 2)
        var userId = "demo-user";

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
        // TODO: Replace with authenticated user ID from Entra ID (Day 2)
        var userId = "demo-user";

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
        // TODO: Replace with authenticated user ID from Entra ID (Day 2)
        var userId = "demo-user";

        var document = await _cosmosService.GetDocumentAsync(id, userId);

        if (document == null || document.Status == "deleted")
            return NotFound();

        await _cosmosService.DeleteDocumentAsync(id, userId);

        _logger.LogInformation("Document {DocumentId} soft-deleted by user {UserId}", id, userId);

        return NoContent();
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
