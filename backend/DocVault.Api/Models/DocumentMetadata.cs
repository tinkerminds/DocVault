using Newtonsoft.Json;

namespace DocVault.Api.Models;

public class DocumentMetadata
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonProperty("fileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonProperty("blobUrl")]
    public string BlobUrl { get; set; } = string.Empty;

    [JsonProperty("contentType")]
    public string ContentType { get; set; } = string.Empty;

    [JsonProperty("sizeBytes")]
    public long SizeBytes { get; set; }

    [JsonProperty("uploadedAt")]
    public string UploadedAt { get; set; } = DateTime.UtcNow.ToString("o");

    [JsonProperty("tags")]
    public List<string> Tags { get; set; } = new();

    [JsonProperty("excerpt")]
    public string? Excerpt { get; set; }

    [JsonProperty("thumbnailUrl")]
    public string? ThumbnailUrl { get; set; }

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; } = "pending";
}
