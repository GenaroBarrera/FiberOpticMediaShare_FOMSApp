using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FOMSApp.API.Data;
using FOMSApp.Shared.Models;
using System.IO.Compression;

namespace FOMSApp.API.Controllers;

// API controller for photo uploads and management.
[Route("api/[controller]")]
[ApiController]
public class PhotosController(AppDbContext context, IWebHostEnvironment env) : ControllerBase
{
    private readonly AppDbContext _context = context;
    private readonly IWebHostEnvironment _env = env;

    // GET: api/photos/vault/{vaultId} - Gets all photos for a specific vault.
    [HttpGet("vault/{vaultId}")]
    public async Task<ActionResult<IEnumerable<Photo>>> GetPhotosForVault(int vaultId)
    {
        return await _context.Photos
            .Where(p => p.VaultId == vaultId)
            .ToListAsync();
    }

    // GET: api/photos/midpoint/{midpointId} - Gets all photos for a specific midpoint.
    [HttpGet("midpoint/{midpointId}")]
    public async Task<ActionResult<IEnumerable<Photo>>> GetPhotosForMidpoint(int midpointId)
    {
        return await _context.Photos
            .Where(p => p.MidpointId == midpointId)
            .ToListAsync();
    }

    // POST: api/photos - Uploads a photo file and creates a database record.
    [HttpPost]
    public async Task<ActionResult<Photo>> UploadPhoto([FromForm] PhotoUploadDto upload)
    {
        if (upload.File == null || upload.File.Length == 0)
            return BadRequest("No file uploaded.");

        if ((upload.VaultId.HasValue && upload.MidpointId.HasValue) ||
            (!upload.VaultId.HasValue && !upload.MidpointId.HasValue))
            return BadRequest("Provide either VaultId or MidpointId, not both.");

        string uploadPath = Path.Combine(_env.WebRootPath, "uploads");
        if (!Directory.Exists(uploadPath))
            Directory.CreateDirectory(uploadPath);

        string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(upload.File.FileName);
        string fullPath = Path.Combine(uploadPath, uniqueFileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await upload.File.CopyToAsync(stream);
        }

        var photo = new Photo
        {
            FileName = uniqueFileName,
            VaultId = upload.VaultId,
            MidpointId = upload.MidpointId,
            UploadedAt = DateTime.Now
        };

        _context.Photos.Add(photo);
        await _context.SaveChangesAsync();

        return Ok(photo);
    }

    // GET: api/photos/vault/{vaultId}/download - Downloads all photos for a vault as a ZIP file.
    [HttpGet("vault/{vaultId}/download")]
    public async Task<IActionResult> DownloadVaultPhotos(int vaultId)
    {
        var photos = await _context.Photos.Where(p => p.VaultId == vaultId).ToListAsync();

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
                string filePath = Path.Combine(_env.WebRootPath, "uploads", photo.FileName);
                if (System.IO.File.Exists(filePath))
                {
                    var entry = archive.CreateEntry($"{photo.Id}_{photo.FileName}");
                    using var entryStream = entry.Open();
                    using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    await fileStream.CopyToAsync(entryStream);
                }
            }
        }

        memoryStream.Position = 0;
        return File(memoryStream.ToArray(), "application/zip", $"{safeVaultName}_Photos.zip");
    }

    // GET: api/photos/midpoint/{midpointId}/download - Downloads all photos for a midpoint as a ZIP file.
    [HttpGet("midpoint/{midpointId}/download")]
    public async Task<IActionResult> DownloadMidpointPhotos(int midpointId)
    {
        var photos = await _context.Photos.Where(p => p.MidpointId == midpointId).ToListAsync();

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
                string filePath = Path.Combine(_env.WebRootPath, "uploads", photo.FileName);
                if (System.IO.File.Exists(filePath))
                {
                    var entry = archive.CreateEntry($"{photo.Id}_{photo.FileName}");
                    using var entryStream = entry.Open();
                    using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    await fileStream.CopyToAsync(entryStream);
                }
            }
        }

        memoryStream.Position = 0;
        return File(memoryStream.ToArray(), "application/zip", $"{safeMidpointName}_Photos.zip");
    }

    // GET: api/photos/batch-download - Downloads photos for multiple vaults/midpoints as a single ZIP file.
    [HttpGet("batch-download")]
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
                var photos = await _context.Photos.Where(p => p.VaultId == vaultId).ToListAsync();
                if (photos.Count > 0)
                {
                    var vault = await _context.Vaults.FindAsync(vaultId);
                    string folderName = string.Join("_", (vault?.Name ?? $"Vault_{vaultId}").Split(Path.GetInvalidFileNameChars())) + "_Photos";
                    await AddPhotosToArchive(archive, photos, folderName);
                }
            }

            foreach (var midpointId in midpointIdList)
            {
                var photos = await _context.Photos.Where(p => p.MidpointId == midpointId).ToListAsync();
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
    public async Task<IActionResult> DeletePhoto(int id)
    {
        var photo = await _context.Photos.FindAsync(id);

        if (photo == null)
            return NotFound();

        string filePath = Path.Combine(_env.WebRootPath, "uploads", photo.FileName);
        if (System.IO.File.Exists(filePath))
        {
            try { System.IO.File.Delete(filePath); }
            catch (Exception ex) { Console.WriteLine($"Warning: Could not delete {filePath}: {ex.Message}"); }
        }

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
            string filePath = Path.Combine(_env.WebRootPath, "uploads", photo.FileName);
            if (System.IO.File.Exists(filePath))
            {
                var entry = archive.CreateEntry($"{folderName}/{photo.Id}_{photo.FileName}");
                using var entryStream = entry.Open();
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                await fileStream.CopyToAsync(entryStream);
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
