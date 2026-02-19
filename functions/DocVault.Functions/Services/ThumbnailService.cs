using Microsoft.Extensions.Logging;
using SkiaSharp;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace DocVault.Functions.Services;

public class ThumbnailService : IThumbnailService
{
    private const int ThumbnailSize = 200;
    private readonly ILogger<ThumbnailService> _logger;

    public ThumbnailService(ILogger<ThumbnailService> logger)
    {
        _logger = logger;
    }

    public async Task<Stream?> GenerateAsync(Stream input, string contentType)
    {
        try
        {
            var lower = contentType.ToLowerInvariant();

            if (lower is "image/jpeg" or "image/jpg" or "image/png" or "image/webp" or "image/gif")
            {
                return await Task.FromResult(GenerateImageThumbnail(input));
            }

            if (lower == "application/pdf")
            {
                return await Task.FromResult(GeneratePdfThumbnail(input));
            }

            _logger.LogInformation("Thumbnail not supported for content type: {ContentType}", contentType);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate thumbnail for content type: {ContentType}", contentType);
            return null;
        }
    }

    // ── Image thumbnail (JPEG / PNG / WEBP / GIF) ──────────────────────────
    private Stream GenerateImageThumbnail(Stream input)
    {
        // Decode original image
        using var original = SKBitmap.Decode(input);
        if (original == null)
            throw new InvalidOperationException("Could not decode image stream.");

        // Calculate proportional size keeping aspect ratio within ThumbnailSize x ThumbnailSize
        var (width, height) = ScaleProportional(original.Width, original.Height, ThumbnailSize);

        // Resize
        using var resized = original.Resize(new SKImageInfo(width, height), SKFilterQuality.High);
        using var image = SKImage.FromBitmap(resized);

        // Encode as JPEG
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 85);

        var output = new MemoryStream();
        data.SaveTo(output);
        output.Position = 0;

        _logger.LogInformation("Image thumbnail generated: {Width}x{Height}", width, height);
        return output;
    }

    // ── PDF thumbnail (first page → text-based placeholder) ────────────────
    private Stream GeneratePdfThumbnail(Stream input)
    {
        // PdfPig: open PDF and read first page words to build a text snapshot
        // Then render a simple 200x200 JPEG with the page number label
        // (Full rasterisation requires a native PDF renderer; PdfPig is text-only)
        using var pdf = PdfDocument.Open(input);
        var page = pdf.GetPage(1);

        // Build a simple grey thumbnail with "PDF" label using SkiaSharp
        using var surface = SKSurface.Create(new SKImageInfo(ThumbnailSize, ThumbnailSize));
        var canvas = surface.Canvas;

        // Background
        canvas.Clear(SKColors.LightGray);

        // Draw "PDF" label
        using var paint = new SKPaint
        {
            Color = SKColors.DarkSlateGray,
            TextSize = 36,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            FakeBoldText = true
        };
        canvas.DrawText("PDF", ThumbnailSize / 2f, ThumbnailSize / 2f + 12, paint);

        // Draw page count
        using var smallPaint = new SKPaint
        {
            Color = SKColors.Gray,
            TextSize = 16,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        };
        canvas.DrawText($"{pdf.NumberOfPages} page(s)", ThumbnailSize / 2f, ThumbnailSize / 2f + 40, smallPaint);

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 85);

        var output = new MemoryStream();
        data.SaveTo(output);
        output.Position = 0;

        _logger.LogInformation("PDF thumbnail generated for {PageCount} page document", pdf.NumberOfPages);
        return output;
    }

    // ── Helpers ─────────────────────────────────────────────────────────────
    private static (int width, int height) ScaleProportional(int origWidth, int origHeight, int maxSize)
    {
        if (origWidth <= 0 || origHeight <= 0) return (maxSize, maxSize);

        var ratio = Math.Min((double)maxSize / origWidth, (double)maxSize / origHeight);
        return ((int)(origWidth * ratio), (int)(origHeight * ratio));
    }
}
