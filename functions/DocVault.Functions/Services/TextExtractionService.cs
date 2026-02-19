using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;

namespace DocVault.Functions.Services;

public class TextExtractionService : ITextExtractionService
{
    private const int MaxExcerptLength = 500;
    private readonly ILogger<TextExtractionService> _logger;

    public TextExtractionService(ILogger<TextExtractionService> logger)
    {
        _logger = logger;
    }

    public async Task<string?> ExtractAsync(Stream input, string contentType)
    {
        try
        {
            var lower = contentType.ToLowerInvariant();

            if (lower == "application/pdf")
            {
                return await Task.FromResult(ExtractFromPdf(input));
            }

            // Plain text files
            if (lower is "text/plain" or "text/csv")
            {
                return await ExtractFromText(input);
            }

            _logger.LogInformation("Text extraction not supported for content type: {ContentType}", contentType);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract text for content type: {ContentType}", contentType);
            return null;
        }
    }

    /// <summary>
    /// Extract text from all PDF pages using PdfPig.
    /// </summary>
    private string? ExtractFromPdf(Stream input)
    {
        using var pdf = PdfDocument.Open(input);
        var allText = new System.Text.StringBuilder();

        foreach (var page in pdf.GetPages())
        {
            var words = page.GetWords();
            foreach (var word in words)
            {
                allText.Append(word.Text);
                allText.Append(' ');
            }

            // Stop early if we already have enough text
            if (allText.Length >= MaxExcerptLength)
                break;
        }

        var result = allText.ToString().Trim();

        if (string.IsNullOrEmpty(result))
        {
            _logger.LogInformation("No text extracted from PDF (might be scanned/image-based)");
            return null;
        }

        // Truncate to 500 chars
        if (result.Length > MaxExcerptLength)
            result = result[..MaxExcerptLength];

        _logger.LogInformation("Extracted {Length} characters from PDF", result.Length);
        return result;
    }

    /// <summary>
    /// Extract text from plain text files.
    /// </summary>
    private async Task<string?> ExtractFromText(Stream input)
    {
        using var reader = new StreamReader(input, leaveOpen: true);
        var content = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(content))
            return null;

        if (content.Length > MaxExcerptLength)
            content = content[..MaxExcerptLength];

        _logger.LogInformation("Extracted {Length} characters from text file", content.Length);
        return content.Trim();
    }
}
