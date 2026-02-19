namespace DocVault.Functions.Services;

public interface ITextExtractionService
{
    /// <summary>
    /// Extract text content from a document stream.
    /// Returns the first 500 characters of extracted text, or null if unsupported.
    /// </summary>
    /// <param name="input">The document file stream</param>
    /// <param name="contentType">MIME type e.g. "application/pdf"</param>
    Task<string?> ExtractAsync(Stream input, string contentType);
}
