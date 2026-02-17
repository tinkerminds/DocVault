namespace DocVault.Api.Models;

public class DocumentUploadResponse
{
    public string Id { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string UploadedAt { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
}
