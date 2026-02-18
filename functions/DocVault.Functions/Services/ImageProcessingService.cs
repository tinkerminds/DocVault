using SkiaSharp;
using System.IO;

namespace DocVault.Functions.Services;

public interface IImageProcessingService
{
    Stream ResizeImage(Stream originalStream, int width, int height);
    bool IsImage(string fileName);
}

public class ImageProcessingService : IImageProcessingService
{
    private readonly string[] _supportedExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };

    public bool IsImage(string fileName)
    {
        var ext = Path.GetExtension(fileName)?.ToLower();
        return !string.IsNullOrEmpty(ext) && _supportedExtensions.Contains(ext);
    }

    public Stream ResizeImage(Stream originalStream, int width, int height)
    {
        // Reset stream position if needed
        if (originalStream.CanSeek)
            originalStream.Position = 0;

        using var originalBitmap = SKBitmap.Decode(originalStream);
        if (originalBitmap == null)
            throw new InvalidOperationException("Could not decode image stream.");

        // Calculate aspect ratio
        float ratioX = (float)width / originalBitmap.Width;
        float ratioY = (float)height / originalBitmap.Height;
        float ratio = Math.Min(ratioX, ratioY);

        int newWidth = (int)(originalBitmap.Width * ratio);
        int newHeight = (int)(originalBitmap.Height * ratio);

        using var resizedBitmap = originalBitmap.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.Medium);
        using var image = SKImage.FromBitmap(resizedBitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 75);

        var outputStream = new MemoryStream();
        data.SaveTo(outputStream);
        outputStream.Position = 0;
        return outputStream;
    }
}
