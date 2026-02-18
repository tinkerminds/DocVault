using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DocVault.Functions.Services;

public interface ITextExtractionService
{
    Task<string> ExtractTextAsync(Stream fileStream, string fileName);
}

public class TextExtractionService : ITextExtractionService
{
    private readonly string[] _supportedExtensions = { ".txt", ".md", ".json", ".xml", ".html", ".css", ".js", ".cs" };

    public async Task<string> ExtractTextAsync(Stream fileStream, string fileName)
    {
        var ext = Path.GetExtension(fileName)?.ToLower();
        if (string.IsNullOrEmpty(ext) || !_supportedExtensions.Contains(ext))
        {
            return string.Empty;
        }

        // Reset stream if possible
        if (fileStream.CanSeek)
            fileStream.Position = 0;

        using var reader = new StreamReader(fileStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
        
        char[] buffer = new char[500];
        int bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length);

        return new string(buffer, 0, bytesRead);
    }
}
