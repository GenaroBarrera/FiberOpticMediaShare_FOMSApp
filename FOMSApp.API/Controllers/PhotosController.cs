using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FOMSApp.API.Data;
using FOMSApp.Shared.Models;
using System.IO.Compression;

namespace FOMSApp.API.Controllers
{
    /// <summary>
    /// RESTful API controller for managing Photo entities and file uploads.
    /// Handles HTTP requests for uploading photos and retrieving photos for specific vaults or midpoints.
    /// </summary>
    /// <remarks>
    /// Endpoints:
    /// - GET /api/photos/vault/{vaultId} - Get all photos for a specific vault
    /// - GET /api/photos/midpoint/{midpointId} - Get all photos for a specific midpoint
    /// - POST /api/photos - Upload a new photo file (for vault or midpoint)
    /// - DELETE /api/photos/{id} - Delete a photo
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        /// <summary>
        /// Database context for accessing photo metadata.
        /// Injected via constructor dependency injection.
        /// </summary>
        private readonly AppDbContext _context;

        /// <summary>
        /// Provides access to the web application's file system paths (e.g., wwwroot folder).
        /// Used to determine where to save uploaded files.
        /// 
        /// IWebHostEnvironment tells us about the hosting environment (Development, Production, etc.)
        /// and provides paths like WebRootPath (typically wwwroot folder).
        /// </summary>
        private readonly IWebHostEnvironment _env;

        /// <summary>
        /// Constructor that receives dependencies via dependency injection.
        /// </summary>
        /// <param name="context">The database context instance</param>
        /// <param name="env">The web hosting environment instance</param>
        public PhotosController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        /// <summary>
        /// Retrieves all photos associated with a specific vault.
        /// </summary>
        /// <param name="vaultId">The unique ID of the vault whose photos should be retrieved</param>
        /// <returns>
        /// HTTP 200 OK with a list of Photo objects (metadata only - file names, not the actual image bytes).
        /// Returns an empty list if the vault has no photos.
        /// </returns>
        /// <remarks>
        /// This endpoint returns Photo records (database metadata), not the actual image files.
        /// The frontend constructs URLs like: /uploads/{FileName} to display the images.
        /// 
        /// Using .Where() filters the photos in the database query (efficient - only matching rows are returned).
        /// </remarks>
        // GET: api/photos/vault/5
        [HttpGet("vault/{vaultId}")] // Custom route: /api/photos/vault/{vaultId} instead of /api/photos/{id}
        public async Task<ActionResult<IEnumerable<Photo>>> GetPhotosForVault(int vaultId)
        {
            // Filter photos by VaultId - Entity Framework translates this to SQL WHERE clause
            return await _context.Photos
                .Where(p => p.VaultId == vaultId) // LINQ expression translated to SQL: WHERE VaultId = @vaultId
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all photos associated with a specific midpoint.
        /// </summary>
        /// <param name="midpointId">The unique ID of the midpoint whose photos should be retrieved</param>
        /// <returns>
        /// HTTP 200 OK with a list of Photo objects (metadata only - file names, not the actual image bytes).
        /// Returns an empty list if the midpoint has no photos.
        /// </returns>
        /// <remarks>
        /// This endpoint returns Photo records (database metadata), not the actual image files.
        /// The frontend constructs URLs like: /uploads/{FileName} to display the images.
        /// 
        /// Using .Where() filters the photos in the database query (efficient - only matching rows are returned).
        /// </remarks>
        // GET: api/photos/midpoint/5
        [HttpGet("midpoint/{midpointId}")] // Custom route: /api/photos/midpoint/{midpointId}
        public async Task<ActionResult<IEnumerable<Photo>>> GetPhotosForMidpoint(int midpointId)
        {
            // Filter photos by MidpointId - Entity Framework translates this to SQL WHERE clause
            return await _context.Photos
                .Where(p => p.MidpointId == midpointId) // LINQ expression translated to SQL: WHERE MidpointId = @midpointId
                .ToListAsync();
        }

        /// <summary>
        /// Uploads a photo file and creates a Photo record in the database.
        /// </summary>
        /// <param name="upload">
        /// A DTO (Data Transfer Object) containing the file and vault ID.
        /// [FromForm] tells ASP.NET Core to read from multipart/form-data (file upload format).
        /// </param>
        /// <returns>
        /// HTTP 200 OK with the created Photo record if successful.
        /// HTTP 400 Bad Request if no file was uploaded or the file is empty.
        /// </returns>
        /// <remarks>
        /// File Upload Process:
        /// 1. Validate the uploaded file
        /// 2. Generate a unique filename (using GUID) to prevent conflicts and security issues
        /// 3. Save the file to wwwroot/uploads folder
        /// 4. Create a Photo record in the database with the filename reference
        /// 5. Return the Photo metadata
        /// 
        /// Security Best Practices:
        /// - Files are renamed with GUIDs (prevents path traversal attacks and naming conflicts)
        /// - File validation should be added (check file type, size limits)
        /// - Consider virus scanning in production
        /// 
        /// Performance Note: File I/O is async to avoid blocking threads during disk writes.
        /// </remarks>
        // POST: api/photos
        [HttpPost]
        public async Task<ActionResult<Photo>> UploadPhoto([FromForm] PhotoUploadDto upload)
        {
            // Validation: Ensure a file was actually uploaded
            if (upload.File == null || upload.File.Length == 0)
                return BadRequest("No file uploaded.");

            // Validation: Ensure either VaultId or MidpointId is provided (but not both)
            if ((upload.VaultId.HasValue && upload.MidpointId.HasValue) || 
                (!upload.VaultId.HasValue && !upload.MidpointId.HasValue))
            {
                return BadRequest("Either VaultId or MidpointId must be provided, but not both.");
            }

            // Determine the upload directory path (typically: wwwroot/uploads)
            // WebRootPath points to the wwwroot folder in your project
            string uploadPath = Path.Combine(_env.WebRootPath, "uploads");
            
            // Create the uploads directory if it doesn't exist
            // This prevents errors on first upload
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Generate a unique filename using GUID to prevent:
            // 1. Naming conflicts (two users upload "photo.jpg")
            // 2. Security issues (path traversal, predictable filenames)
            // Keep the original extension so browsers know how to display it
            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(upload.File.FileName);
            string fullPath = Path.Combine(uploadPath, uniqueFileName);

            // Save the file to disk asynchronously
            // Using statement ensures the file stream is properly disposed (released) after writing
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                // Copy the uploaded file content to disk
                // This is async to avoid blocking the thread during I/O operations
                await upload.File.CopyToAsync(stream);
            }

            // Create a Photo record in the database
            // We store only the filename, not the file bytes (keeps database small)
            var photo = new Photo
            {
                FileName = uniqueFileName, // Store the GUID-based filename
                VaultId = upload.VaultId, // Link to the parent vault (nullable)
                MidpointId = upload.MidpointId, // Link to the parent midpoint (nullable)
                UploadedAt = DateTime.Now // Record when the upload occurred
            };

            // Add to database and save
            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            // Return the created Photo record (includes the auto-generated ID)
            return Ok(photo);
        }

        /// <summary>
        /// Downloads all photos for a specific vault as a ZIP file.
        /// </summary>
        /// <param name="vaultId">The unique ID of the vault whose photos should be downloaded</param>
        /// <returns>
        /// HTTP 200 OK with a ZIP file containing all photos, or HTTP 404 Not Found if the vault has no photos.
        /// </returns>
        /// <remarks>
        /// This endpoint creates a ZIP archive containing all photos associated with the vault.
        /// The ZIP file is created in memory and streamed directly to the client.
        /// Photos are named using their original filename or a sequential number if the original name is not available.
        /// </remarks>
        // GET: api/photos/vault/5/download
        [HttpGet("vault/{vaultId}/download")]
        public async Task<IActionResult> DownloadVaultPhotos(int vaultId)
        {
            // Get all photos for this vault
            var photos = await _context.Photos
                .Where(p => p.VaultId == vaultId)
                .ToListAsync();

            if (photos == null || photos.Count == 0)
            {
                return NotFound("No photos found for this vault.");
            }

            // Get the vault name for the ZIP filename
            var vault = await _context.Vaults.FindAsync(vaultId);
            string vaultName = vault?.Name ?? $"Vault_{vaultId}";
            // Sanitize the vault name for use in filename (remove invalid characters)
            string safeVaultName = string.Join("_", vaultName.Split(Path.GetInvalidFileNameChars()));

            // Create a memory stream for the ZIP file
            using var memoryStream = new MemoryStream();
            
            // Create a ZIP archive in the memory stream
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var photo in photos)
                {
                    string filePath = Path.Combine(_env.WebRootPath, "uploads", photo.FileName);
                    
                    // Only add files that actually exist on disk
                    if (System.IO.File.Exists(filePath))
                    {
                        // Create an entry in the ZIP archive
                        // Use the photo's ID and original extension, or a sequential number
                        string entryName = $"{photo.Id}_{photo.FileName}";
                        var entry = archive.CreateEntry(entryName);
                        
                        // Copy the file content into the ZIP entry
                        using (var entryStream = entry.Open())
                        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                            await fileStream.CopyToAsync(entryStream);
                        }
                    }
                }
            }

            // Reset the stream position to the beginning
            memoryStream.Position = 0;

            // Return the ZIP file with appropriate headers
            return File(memoryStream.ToArray(), "application/zip", $"{safeVaultName}_Photos.zip");
        }

        /// <summary>
        /// Downloads all photos for a specific midpoint as a ZIP file.
        /// </summary>
        /// <param name="midpointId">The unique ID of the midpoint whose photos should be downloaded</param>
        /// <returns>
        /// HTTP 200 OK with a ZIP file containing all photos, or HTTP 404 Not Found if the midpoint has no photos.
        /// </returns>
        /// <remarks>
        /// This endpoint creates a ZIP archive containing all photos associated with the midpoint.
        /// The ZIP file is created in memory and streamed directly to the client.
        /// Photos are named using their original filename or a sequential number if the original name is not available.
        /// </remarks>
        // GET: api/photos/midpoint/5/download
        [HttpGet("midpoint/{midpointId}/download")]
        public async Task<IActionResult> DownloadMidpointPhotos(int midpointId)
        {
            // Get all photos for this midpoint
            var photos = await _context.Photos
                .Where(p => p.MidpointId == midpointId)
                .ToListAsync();

            if (photos == null || photos.Count == 0)
            {
                return NotFound("No photos found for this midpoint.");
            }

            // Get the midpoint name for the ZIP filename
            var midpoint = await _context.Midpoints.FindAsync(midpointId);
            string midpointName = midpoint?.Name ?? $"Midpoint_{midpointId}";
            // Sanitize the midpoint name for use in filename (remove invalid characters)
            string safeMidpointName = string.Join("_", midpointName.Split(Path.GetInvalidFileNameChars()));

            // Create a memory stream for the ZIP file
            using var memoryStream = new MemoryStream();
            
            // Create a ZIP archive in the memory stream
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var photo in photos)
                {
                    string filePath = Path.Combine(_env.WebRootPath, "uploads", photo.FileName);
                    
                    // Only add files that actually exist on disk
                    if (System.IO.File.Exists(filePath))
                    {
                        // Create an entry in the ZIP archive
                        // Use the photo's ID and original extension, or a sequential number
                        string entryName = $"{photo.Id}_{photo.FileName}";
                        var entry = archive.CreateEntry(entryName);
                        
                        // Copy the file content into the ZIP entry
                        using (var entryStream = entry.Open())
                        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                            await fileStream.CopyToAsync(entryStream);
                        }
                    }
                }
            }

            // Reset the stream position to the beginning
            memoryStream.Position = 0;

            // Return the ZIP file with appropriate headers
            return File(memoryStream.ToArray(), "application/zip", $"{safeMidpointName}_Photos.zip");
        }

        /// <summary>
        /// Deletes a photo from the database and removes the physical file from disk.
        /// </summary>
        /// <param name="id">The unique ID of the photo to delete (from the URL route)</param>
        /// <returns>
        /// HTTP 204 No Content if successfully deleted, or HTTP 404 Not Found if the photo doesn't exist.
        /// </returns>
        /// <remarks>
        /// This method performs two operations:
        /// 1. Deletes the Photo record from the database
        /// 2. Deletes the physical image file from the wwwroot/uploads folder
        /// 
        /// If the file doesn't exist on disk, the database record is still deleted (prevents orphaned records).
        /// This is a "best effort" cleanup - we don't fail if the file is already missing.
        /// 
        /// RESTful Best Practice: DELETE operations typically return 204 No Content (success with no body)
        /// rather than 200 OK, because there's nothing meaningful to return after deletion.
        /// </remarks>
        // DELETE: api/photos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int id)
        {
            // Find the photo by primary key
            var photo = await _context.Photos.FindAsync(id);
            
            // Return 404 if not found (standard REST practice)
            if (photo == null)
                return NotFound();

            // Delete the physical file from disk (if it exists)
            // Use System.IO.File to avoid conflict with ControllerBase.File method
            string filePath = Path.Combine(_env.WebRootPath, "uploads", photo.FileName);
            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    System.IO.File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    // Log the error but continue with database deletion
                    // This prevents orphaned database records if file deletion fails
                    Console.WriteLine($"Warning: Could not delete file {filePath}: {ex.Message}");
                }
            }

            // Mark the entity for deletion
            _context.Photos.Remove(photo);
            
            // Execute the SQL DELETE statement
            await _context.SaveChangesAsync();
            
            // Return 204 No Content (successful deletion with no response body)
            return NoContent();
        }
    }

    /// <summary>
    /// Data Transfer Object (DTO) for photo uploads.
    /// Wraps the file and entity ID (vault or midpoint) together in a single object that ASP.NET Core can deserialize.
    /// </summary>
    /// <remarks>
    /// Why use a DTO instead of Photo?
    /// - Photo model doesn't have an IFormFile property (files come from HTTP multipart/form-data)
    /// - DTOs provide a clean separation between API contracts and database models
    /// - Makes Swagger documentation clearer
    /// 
    /// The [FromForm] attribute in the controller tells ASP.NET Core to bind this from multipart/form-data,
    /// which is the format browsers use when submitting files via HTML forms.
    /// 
    /// Note: Either VaultId OR MidpointId must be provided, but not both.
    /// </remarks>
    public class PhotoUploadDto
    {
        /// <summary>
        /// The ID of the vault this photo belongs to (optional).
        /// Must match an existing Vault.Id in the database if provided.
        /// Either VaultId or MidpointId must be set, but not both.
        /// </summary>
        public int? VaultId { get; set; }

        /// <summary>
        /// The ID of the midpoint this photo belongs to (optional).
        /// Must match an existing Midpoint.Id in the database if provided.
        /// Either VaultId or MidpointId must be set, but not both.
        /// </summary>
        public int? MidpointId { get; set; }

        /// <summary>
        /// The uploaded image file from the HTTP request.
        /// IFormFile is ASP.NET Core's representation of an uploaded file.
        /// It provides access to the file stream, filename, content type, and size.
        /// </summary>
        public IFormFile? File { get; set; }
    }
}