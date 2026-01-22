using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace FOMSApp.API.Services;

/// <summary>
/// Azure Blob Storage implementation for file storage.
/// </summary>
public class AzureBlobStorageService : IStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;
    private readonly ILogger<AzureBlobStorageService> _logger;
    private readonly string? _baseUrl;

    public AzureBlobStorageService(
        BlobServiceClient blobServiceClient,
        string containerName,
        ILogger<AzureBlobStorageService> logger,
        string? baseUrl = null)
    {
        _blobServiceClient = blobServiceClient;
        _containerName = containerName;
        _logger = logger;
        _baseUrl = baseUrl;

        // Ensure container exists (fire and forget - will be created on first use if this fails)
        _ = Task.Run(async () =>
        {
            try
            {
                await InitializeContainerAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to initialize Azure Blob Storage container during startup. It will be created on first use.");
            }
        });
    }

    private async Task InitializeContainerAsync()
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        // Use Private access (no public access) - photos will be served through the API
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);
        _logger.LogInformation("Azure Blob Storage container '{ContainerName}' is ready", _containerName);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
        var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;

        try
        {
            // Ensure container exists before upload
            await InitializeContainerAsync();
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(uniqueFileName);

            // Reset stream position if needed
            if (fileStream.CanSeek && fileStream.Position > 0)
            {
                fileStream.Position = 0;
            }

            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                }
            };

            await blobClient.UploadAsync(fileStream, uploadOptions);
            _logger.LogDebug("Uploaded file to Azure Blob Storage: {FileName}", uniqueFileName);

            return uniqueFileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to Azure Blob Storage: {FileName}", fileName);
            throw;
        }
    }

    public async Task<Stream?> DownloadFileAsync(string fileName)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            if (!await blobClient.ExistsAsync())
            {
                _logger.LogWarning("File not found in Azure Blob Storage: {FileName}", fileName);
                return null;
            }

            var memoryStream = new MemoryStream();
            await blobClient.DownloadToAsync(memoryStream);
            memoryStream.Position = 0;

            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file from Azure Blob Storage: {FileName}", fileName);
            return null;
        }
    }

    public async Task<bool> DeleteFileAsync(string fileName)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            var response = await blobClient.DeleteIfExistsAsync();
            
            if (response.Value)
            {
                _logger.LogDebug("Deleted file from Azure Blob Storage: {FileName}", fileName);
                return true;
            }
            else
            {
                _logger.LogWarning("File not found for deletion in Azure Blob Storage: {FileName}", fileName);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from Azure Blob Storage: {FileName}", fileName);
            return false;
        }
    }

    public async Task<bool> FileExistsAsync(string fileName)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            return await blobClient.ExistsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence in Azure Blob Storage: {FileName}", fileName);
            return false;
        }
    }

    public string GetFileUrl(string fileName)
    {
        if (!string.IsNullOrEmpty(_baseUrl))
        {
            // Use provided base URL (useful for CDN or custom domain)
            return $"{_baseUrl.TrimEnd('/')}/{_containerName}/{fileName}";
        }

        // Construct URL from blob client
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(fileName);
        return blobClient.Uri.ToString();
    }
}
