namespace DocVault.Api.Services;

public interface IBlobStorageService
{
    /// <summary>
    /// Upload a file to Blob Storage.
    /// </summary>
    /// <param name="stream">File content stream</param>
    /// <param name="fileName">Original file name</param>
    /// <param name="contentType">MIME type</param>
    /// <param name="userId">Uploader's user ID (used as blob path prefix)</param>
    /// <returns>The full blob URL of the uploaded file</returns>
    Task<string> UploadAsync(Stream stream, string fileName, string contentType, string userId);

    /// <summary>
    /// Generate a time-limited SAS download URL for a blob.
    /// </summary>
    /// <param name="blobUrl">Full blob URL</param>
    /// <returns>SAS URL valid for a limited time</returns>
    string GetSasDownloadUrl(string blobUrl);

    /// <summary>
    /// Delete a blob from storage.
    /// </summary>
    /// <param name="blobUrl">Full blob URL of the blob to delete</param>
    Task DeleteAsync(string blobUrl);
}
