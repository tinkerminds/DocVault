namespace DocVault.Api.Models;

/// <summary>
/// Clean DTO returned by the document search endpoint.
/// Never exposes internal database model fields.
/// </summary>
public class DocumentSearchDto
{
    /// <summary>Cosmos document ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Original file name as uploaded by the user.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>URL of the generated thumbnail image, if available.</summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>Extracted text excerpt (up to 300 chars), if available.</summary>
    public string? Excerpt { get; set; }

    /// <summary>ISO-8601 UTC timestamp of when the document was uploaded.</summary>
    public string UploadedAt { get; set; } = string.Empty;
}
