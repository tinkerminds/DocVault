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

    // ── PDF thumbnail (render actual first page) ────────────────────────────
    private Stream GeneratePdfThumbnail(Stream input)
    {
        try
        {
            // Read the stream into a byte array for Docnet
            byte[] pdfBytes;
            if (input is MemoryStream ms)
            {
                pdfBytes = ms.ToArray();
            }
            else
            {
                using var temp = new MemoryStream();
                input.CopyTo(temp);
                pdfBytes = temp.ToArray();
            }

            // Render the first page at higher resolution for quality
            var renderWidth = ThumbnailSize * 4;
            var renderHeight = ThumbnailSize * 4;

            using var library = Docnet.Core.DocLib.Instance;
            using var docReader = library.GetDocReader(
                pdfBytes,
                new Docnet.Core.Models.PageDimensions(renderWidth, renderHeight));
            using var pageReader = docReader.GetPageReader(0);

            var rawBytes = pageReader.GetImage();
            var pageWidth = pageReader.GetPageWidth();
            var pageHeight = pageReader.GetPageHeight();

            // Convert raw BGRA pixel data to SkiaSharp bitmap
            var info = new SKImageInfo(pageWidth, pageHeight, SKColorType.Bgra8888, SKAlphaType.Premul);
            using var bitmap = new SKBitmap(info);

            var handle = System.Runtime.InteropServices.GCHandle.Alloc(rawBytes, System.Runtime.InteropServices.GCHandleType.Pinned);
            try
            {
                bitmap.InstallPixels(info, handle.AddrOfPinnedObject(), info.RowBytes);
            }
            finally
            {
                handle.Free();
            }

            // Resize to thumbnail dimensions keeping aspect ratio
            var (width, height) = ScaleProportional(pageWidth, pageHeight, ThumbnailSize);
            using var resized = bitmap.Resize(new SKImageInfo(width, height), SKFilterQuality.High);
            using var image = SKImage.FromBitmap(resized);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 85);

            var output = new MemoryStream();
            data.SaveTo(output);
            output.Position = 0;

            _logger.LogInformation("PDF first-page thumbnail generated: {Width}x{Height}", width, height);
            return output;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Docnet rendering failed, falling back to placeholder thumbnail");
            return GeneratePdfPlaceholderThumbnail(input);
        }
    }

    // ── Fallback PDF placeholder thumbnail ─────────────────────────────────
    private Stream GeneratePdfPlaceholderThumbnail(Stream input)
    {
        int pageCount;
        try
        {
            input.Position = 0;
            using var pdf = PdfDocument.Open(input);
            pageCount = pdf.NumberOfPages;
        }
        catch
        {
            pageCount = 0;
        }

        using var surface = SKSurface.Create(new SKImageInfo(ThumbnailSize, ThumbnailSize));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.LightGray);

        using var paint = new SKPaint
        {
            Color = SKColors.DarkSlateGray,
            TextSize = 36,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            FakeBoldText = true
        };
        canvas.DrawText("PDF", ThumbnailSize / 2f, ThumbnailSize / 2f + 12, paint);

        if (pageCount > 0)
        {
            using var smallPaint = new SKPaint
            {
                Color = SKColors.Gray,
                TextSize = 16,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            };
            canvas.DrawText($"{pageCount} page(s)", ThumbnailSize / 2f, ThumbnailSize / 2f + 40, smallPaint);
        }

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 85);

        var output = new MemoryStream();
        data.SaveTo(output);
        output.Position = 0;

        _logger.LogInformation("PDF placeholder thumbnail generated");
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
