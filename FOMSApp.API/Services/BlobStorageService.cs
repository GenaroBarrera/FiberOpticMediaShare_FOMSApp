using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace FOMSApp.API.Services;

// Service for managing photo storage in Azure Blob Storage.
// Falls back to local file system if Azure connection string is not configured.
public class BlobStorageService
{
    private readonly BlobServiceClient? _blobServiceClient;
    private readonly string _containerName;
    private readonly IWebHostEnvironment _env;
    private readonly bool _useBlobStorage;

    public BlobStorageService(IConfiguration configuration, IWebHostEnvironment env)
    {
        _env = env;
        _containerName = configuration["Azure:Storage:ContainerName"] ?? "photos";
        var connectionString = configuration["Azure:Storage:ConnectionString"];

        _useBlobStorage = !string.IsNullOrWhiteSpace(connectionString);

        if (_useBlobStorage)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
            InitializeContainerAsync().GetAwaiter().GetResult();
        }
    }

    // Initializes the blob container if it doesn't exist.
    private async Task InitializeContainerAsync()
    {
        if (_blobServiceClient == null) return;

        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not initialize blob container: {ex.Message}");
        }
    }

    // Uploads a file stream to blob storage and returns the blob URL.
    public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
    {
        if (!_useBlobStorage || _blobServiceClient == null)
        {
            // Fallback to local storage
            return await UploadToLocalStorageAsync(fileStream, fileName);
        }

        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            await blobClient.UploadAsync(fileStream, overwrite: true);
            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading to blob storage: {ex.Message}. Falling back to local storage.");
            return await UploadToLocalStorageAsync(fileStream, fileName);
        }
    }

    // Downloads a file from blob storage.
    public async Task<Stream?> DownloadFileAsync(string fileName)
    {
        if (!_useBlobStorage || _blobServiceClient == null)
        {
            // Fallback to local storage
            return await DownloadFromLocalStorageAsync(fileName);
        }

        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            if (!await blobClient.ExistsAsync())
                return null;

            var memoryStream = new MemoryStream();
            await blobClient.DownloadToAsync(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading from blob storage: {ex.Message}. Falling back to local storage.");
            return await DownloadFromLocalStorageAsync(fileName);
        }
    }

    // Gets the URL for a blob (for direct access).
    public string GetBlobUrl(string fileName)
    {
        if (!_useBlobStorage || _blobServiceClient == null)
        {
            // Return local file URL
            return $"/uploads/{fileName}";
        }

        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            return blobClient.Uri.ToString();
        }
        catch
        {
            return $"/uploads/{fileName}";
        }
    }

    // Deletes a file from blob storage.
    public async Task<bool> DeleteFileAsync(string fileName)
    {
        if (!_useBlobStorage || _blobServiceClient == null)
        {
            // Fallback to local storage
            return await DeleteFromLocalStorageAsync(fileName);
        }

        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            if (await blobClient.ExistsAsync())
            {
                await blobClient.DeleteAsync();
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting from blob storage: {ex.Message}. Falling back to local storage.");
            return await DeleteFromLocalStorageAsync(fileName);
        }
    }

    // Local storage fallback methods
    private async Task<string> UploadToLocalStorageAsync(Stream fileStream, string fileName)
    {
        string uploadPath = Path.Combine(_env.WebRootPath, "uploads");
        if (!Directory.Exists(uploadPath))
            Directory.CreateDirectory(uploadPath);

        string fullPath = Path.Combine(uploadPath, fileName);
        using (var outputStream = new FileStream(fullPath, FileMode.Create))
        {
            await fileStream.CopyToAsync(outputStream);
        }
        return $"/uploads/{fileName}";
    }

    private async Task<Stream?> DownloadFromLocalStorageAsync(string fileName)
    {
        string filePath = Path.Combine(_env.WebRootPath, "uploads", fileName);
        if (!System.IO.File.Exists(filePath))
            return null;

        var memoryStream = new MemoryStream();
        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            await fileStream.CopyToAsync(memoryStream);
        }
        memoryStream.Position = 0;
        return memoryStream;
    }

    private async Task<bool> DeleteFromLocalStorageAsync(string fileName)
    {
        string filePath = Path.Combine(_env.WebRootPath, "uploads", fileName);
        if (System.IO.File.Exists(filePath))
        {
            try
            {
                await Task.Run(() => System.IO.File.Delete(filePath));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not delete {filePath}: {ex.Message}");
                return false;
            }
        }
        return false;
    }
}
