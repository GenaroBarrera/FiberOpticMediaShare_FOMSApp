using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FOMSApp.API.Data;
using FOMSApp.API.Services;
using FOMSApp.Shared.Models;
using System.IO.Compression;

namespace FOMSApp.API.Controllers;

// API controller for photo uploads and management.
[Route("api/[controller]")]
[ApiController]
public class PhotosController(AppDbContext context, IStorageService storageService, ILogger<PhotosController> logger) : ControllerBase
{
    private readonly AppDbContext _context = context;
    private readonly IStorageService _storageService = storageService;
    private readonly ILogger<PhotosController> _logger = logger;

    // GET: api/photos/vault/{vaultId} - Gets all photos for a specific vault.
    [HttpGet("vault/{vaultId}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Photo>>> GetPhotosForVault(int vaultId)
    {
        return await _context.Photos
            .Where(p => p.VaultId == vaultId)
            .AsNoTracking()
            .ToListAsync();
    }

    // GET: api/photos/midpoint/{midpointId} - Gets all photos for a specific midpoint.
    [HttpGet("midpoint/{midpointId}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Photo>>> GetPhotosForMidpoint(int midpointId)
    {
        return await _context.Photos
            .Where(p => p.MidpointId == midpointId)
            .AsNoTracking()
            .ToListAsync();
    }

    // GET: api/photos/file/{fileName} - Serves a photo file from storage.
    [HttpGet("file/{fileName}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPhotoFile(string fileName)
    {
        // Security: Prevent path traversal attacks
        if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
        {
            return BadRequest("Invalid file name.");
        }

        var fileStream = await _storageService.DownloadFileAsync(fileName);
        if (fileStream == null)
        {
            return NotFound();
        }

        // Determine content type from file extension
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var contentType = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

        return File(fileStream, contentType);
    }

    // POST: api/photos - Uploads a photo file and creates a database record.
    [HttpPost]
    [Authorize(Policy = "RequireEditor")]
    public async Task<ActionResult<Photo>> UploadPhoto([FromForm] PhotoUploadDto upload)
    {
        if (upload.File == null || upload.File.Length == 0)
            return BadRequest("No file uploaded.");

        if ((upload.VaultId.HasValue && upload.MidpointId.HasValue) ||
            (!upload.VaultId.HasValue && !upload.MidpointId.HasValue))
            return BadRequest("Provide either VaultId or MidpointId, not both.");

        // Validate file type (allow common image formats)
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        var fileExtension = Path.GetExtension(upload.File.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
        {
            return BadRequest($"File type not allowed. Allowed types: {string.Join(", ", allowedExtensions)}");
        }

        // Validate file size (20MB max)
        const long maxFileSize = 20 * 1024 * 1024; // 20MB
        if (upload.File.Length > maxFileSize)
        {
            return BadRequest($"File size exceeds maximum allowed size of {maxFileSize / (1024 * 1024)}MB.");
        }

        try
        {
            // Upload file using storage service
            string uniqueFileName = await _storageService.UploadFileAsync(
                upload.File.OpenReadStream(),
                upload.File.FileName,
                upload.File.ContentType);

            var photo = new Photo
            {
                FileName = uniqueFileName,
                VaultId = upload.VaultId,
                MidpointId = upload.MidpointId,
                UploadedAt = DateTime.Now
            };

            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Uploaded photo with ID: {PhotoId} for {EntityType} ID: {EntityId}", 
                photo.Id, upload.VaultId.HasValue ? "Vault" : "Midpoint", 
                upload.VaultId ?? upload.MidpointId ?? 0);

            return Ok(photo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading photo: {Message}\n{StackTrace}", ex.Message, ex.StackTrace);
            // Return detailed error for debugging (we can make this conditional later)
            return StatusCode(500, new { 
                error = "An error occurred while uploading the photo.",
                message = ex.Message,
                innerException = ex.InnerException?.Message
            });
        }
    }

    // GET: api/photos/vault/{vaultId}/download - Downloads all photos for a vault as a ZIP file.
    [HttpGet("vault/{vaultId}/download")]
    [AllowAnonymous]
    public async Task<IActionResult> DownloadVaultPhotos(int vaultId)
    {
        var photos = await _context.Photos
            .Where(p => p.VaultId == vaultId)
            .AsNoTracking()
            .ToListAsync();

        if (photos.Count == 0)
            return NotFound("No photos found for this vault.");

        var vault = await _context.Vaults.FindAsync(vaultId);
        string vaultName = vault?.Name ?? $"Vault_{vaultId}";
        string safeVaultName = string.Join("_", vaultName.Split(Path.GetInvalidFileNameChars()));

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var photo in photos)
            {
                var fileStream = await _storageService.DownloadFileAsync(photo.FileName);
                if (fileStream != null)
                {
                    var entry = archive.CreateEntry($"{photo.Id}_{photo.FileName}");
                    using var entryStream = entry.Open();
                    await fileStream.CopyToAsync(entryStream);
                    await fileStream.DisposeAsync();
                }
            }
        }

        memoryStream.Position = 0;
        return File(memoryStream.ToArray(), "application/zip", $"{safeVaultName}_Photos.zip");
    }

    // GET: api/photos/midpoint/{midpointId}/download - Downloads all photos for a midpoint as a ZIP file.
    [HttpGet("midpoint/{midpointId}/download")]
    [AllowAnonymous]
    public async Task<IActionResult> DownloadMidpointPhotos(int midpointId)
    {
        var photos = await _context.Photos
            .Where(p => p.MidpointId == midpointId)
            .AsNoTracking()
            .ToListAsync();

        if (photos.Count == 0)
            return NotFound("No photos found for this midpoint.");

        var midpoint = await _context.Midpoints.FindAsync(midpointId);
        string midpointName = midpoint?.Name ?? $"Midpoint_{midpointId}";
        string safeMidpointName = string.Join("_", midpointName.Split(Path.GetInvalidFileNameChars()));

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var photo in photos)
            {
                var fileStream = await _storageService.DownloadFileAsync(photo.FileName);
                if (fileStream != null)
                {
                    var entry = archive.CreateEntry($"{photo.Id}_{photo.FileName}");
                    using var entryStream = entry.Open();
                    await fileStream.CopyToAsync(entryStream);
                    await fileStream.DisposeAsync();
                }
            }
        }

        memoryStream.Position = 0;
        return File(memoryStream.ToArray(), "application/zip", $"{safeMidpointName}_Photos.zip");
    }

    // GET: api/photos/batch-download - Downloads photos for multiple vaults/midpoints as a single ZIP file.
    [HttpGet("batch-download")]
    [AllowAnonymous]
    public async Task<IActionResult> BatchDownloadPhotos([FromQuery] string? vaultIds, [FromQuery] string? midpointIds)
    {
        var vaultIdList = ParseIds(vaultIds);
        var midpointIdList = ParseIds(midpointIds);

        if (vaultIdList.Count == 0 && midpointIdList.Count == 0)
            return BadRequest("At least one vault ID or midpoint ID must be provided.");

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var vaultId in vaultIdList)
            {
                var photos = await _context.Photos
                    .Where(p => p.VaultId == vaultId)
                    .AsNoTracking()
                    .ToListAsync();
                if (photos.Count > 0)
                {
                    var vault = await _context.Vaults.FindAsync(vaultId);
                    string folderName = string.Join("_", (vault?.Name ?? $"Vault_{vaultId}").Split(Path.GetInvalidFileNameChars())) + "_Photos";
                    await AddPhotosToArchive(archive, photos, folderName);
                }
            }

            foreach (var midpointId in midpointIdList)
            {
                var photos = await _context.Photos
                    .Where(p => p.MidpointId == midpointId)
                    .AsNoTracking()
                    .ToListAsync();
                if (photos.Count > 0)
                {
                    var midpoint = await _context.Midpoints.FindAsync(midpointId);
                    string folderName = string.Join("_", (midpoint?.Name ?? $"Midpoint_{midpointId}").Split(Path.GetInvalidFileNameChars())) + "_Photos";
                    await AddPhotosToArchive(archive, photos, folderName);
                }
            }
        }

        memoryStream.Position = 0;
        int totalEntities = vaultIdList.Count + midpointIdList.Count;
        string zipFileName = totalEntities == 1 ? "Selected_Photos.zip" : $"Selected_Photos_{totalEntities}_Items.zip";

        return File(memoryStream.ToArray(), "application/zip", zipFileName);
    }

    // DELETE: api/photos/{id} - Deletes a photo from the database and file system.
    [HttpDelete("{id}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> DeletePhoto(int id)
    {
        var photo = await _context.Photos.FindAsync(id);

        if (photo == null)
            return NotFound();

        // Delete file using storage service
        await _storageService.DeleteFileAsync(photo.FileName);

        _context.Photos.Remove(photo);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static List<int> ParseIds(string? idString)
    {
        var ids = new List<int>();
        if (string.IsNullOrWhiteSpace(idString)) return ids;

        foreach (var idStr in idString.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            if (int.TryParse(idStr.Trim(), out int id))
                ids.Add(id);
        }
        return ids;
    }

    private async Task AddPhotosToArchive(ZipArchive archive, List<Photo> photos, string folderName)
    {
        foreach (var photo in photos)
        {
            var fileStream = await _storageService.DownloadFileAsync(photo.FileName);
            if (fileStream != null)
            {
                var entry = archive.CreateEntry($"{folderName}/{photo.Id}_{photo.FileName}");
                using var entryStream = entry.Open();
                await fileStream.CopyToAsync(entryStream);
                await fileStream.DisposeAsync();
            }
        }
    }
}

// DTO for photo uploads. Either VaultId or MidpointId must be provided.
public class PhotoUploadDto
{
    public int? VaultId { get; set; }
    public int? MidpointId { get; set; }
    public IFormFile? File { get; set; }
}
