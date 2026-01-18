using Microsoft.AspNetCore.Hosting;

namespace FOMSApp.API.Services;

/// <summary>
/// Local file system storage implementation. Stores files in the wwwroot/uploads directory.
/// </summary>
public class LocalFileStorageService : IStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly string _uploadPath;

    public LocalFileStorageService(IWebHostEnvironment env, ILogger<LocalFileStorageService> logger)
    {
        _env = env;
        _logger = logger;
        _uploadPath = Path.Combine(_env.WebRootPath, "uploads");
        
        // Ensure upload directory exists
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
            _logger.LogInformation("Created upload directory: {UploadPath}", _uploadPath);
        }
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
        var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
        var fullPath = Path.Combine(_uploadPath, uniqueFileName);

        using (var outputStream = new FileStream(fullPath, FileMode.Create))
        {
            await fileStream.CopyToAsync(outputStream);
        }

        _logger.LogDebug("Uploaded file to local storage: {FileName}", uniqueFileName);
        return uniqueFileName;
    }

    public async Task<Stream?> DownloadFileAsync(string fileName)
    {
        var fullPath = Path.Combine(_uploadPath, fileName);
        
        if (!File.Exists(fullPath))
        {
            _logger.LogWarning("File not found in local storage: {FileName}", fileName);
            return null;
        }

        return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    public async Task<bool> DeleteFileAsync(string fileName)
    {
        var fullPath = Path.Combine(_uploadPath, fileName);
        
        if (!File.Exists(fullPath))
        {
            _logger.LogWarning("File not found for deletion: {FileName}", fileName);
            return false;
        }

        try
        {
            await Task.Run(() => File.Delete(fullPath));
            _logger.LogDebug("Deleted file from local storage: {FileName}", fileName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from local storage: {FileName}", fileName);
            return false;
        }
    }

    public Task<bool> FileExistsAsync(string fileName)
    {
        var fullPath = Path.Combine(_uploadPath, fileName);
        return Task.FromResult(File.Exists(fullPath));
    }

    public string GetFileUrl(string fileName)
    {
        // Return relative path for local storage (served via static files middleware)
        return $"/uploads/{fileName}";
    }
}
