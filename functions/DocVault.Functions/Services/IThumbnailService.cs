namespace DocVault.Functions.Services;

public interface IThumbnailService
{
    /// <summary>
    /// Generate a 200x200 JPEG thumbnail from the given stream.
    /// Returns null if the content type is not supported.
    /// </summary>
    /// <param name="input">The original file stream (must be readable + seekable)</param>
    /// <param name="contentType">MIME type e.g. "image/jpeg", "application/pdf"</param>
    Task<Stream?> GenerateAsync(Stream input, string contentType);
}
