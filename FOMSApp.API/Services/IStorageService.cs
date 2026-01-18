namespace FOMSApp.API.Services;

/// <summary>
/// Abstraction for file storage operations. Supports both local file system and Azure Blob Storage.
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Uploads a file and returns the unique file name.
    /// </summary>
    /// <param name="fileStream">The file stream to upload</param>
    /// <param name="fileName">The original file name</param>
    /// <param name="contentType">The content type of the file</param>
    /// <returns>The unique file name used for storage</returns>
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);

    /// <summary>
    /// Downloads a file by its file name.
    /// </summary>
    /// <param name="fileName">The file name to download</param>
    /// <returns>The file stream, or null if the file doesn't exist</returns>
    Task<Stream?> DownloadFileAsync(string fileName);

    /// <summary>
    /// Deletes a file by its file name.
    /// </summary>
    /// <param name="fileName">The file name to delete</param>
    /// <returns>True if the file was deleted, false if it didn't exist</returns>
    Task<bool> DeleteFileAsync(string fileName);

    /// <summary>
    /// Checks if a file exists.
    /// </summary>
    /// <param name="fileName">The file name to check</param>
    /// <returns>True if the file exists, false otherwise</returns>
    Task<bool> FileExistsAsync(string fileName);

    /// <summary>
    /// Gets the URL or path to access a file. For local storage, returns a relative path. For Azure, returns a blob URL.
    /// </summary>
    /// <param name="fileName">The file name</param>
    /// <returns>The URL or path to access the file</returns>
    string GetFileUrl(string fileName);
}
