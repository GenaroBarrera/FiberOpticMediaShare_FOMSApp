using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FOMSApp.API.Data;
using FOMSApp.Shared.Models;

namespace FOMSApp.API.Controllers
{
    /// <summary>
    /// RESTful API controller for managing Photo entities and file uploads.
    /// Handles HTTP requests for uploading photos and retrieving photos for specific vaults.
    /// </summary>
    /// <remarks>
    /// Endpoints:
    /// - GET /api/photos/vault/{vaultId} - Get all photos for a specific vault
    /// - POST /api/photos - Upload a new photo file
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
                VaultId = upload.VaultId, // Link to the parent vault
                UploadedAt = DateTime.Now // Record when the upload occurred
            };

            // Add to database and save
            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            // Return the created Photo record (includes the auto-generated ID)
            return Ok(photo);
        }
    }

    /// <summary>
    /// Data Transfer Object (DTO) for photo uploads.
    /// Wraps the file and vault ID together in a single object that ASP.NET Core can deserialize.
    /// </summary>
    /// <remarks>
    /// Why use a DTO instead of Photo?
    /// - Photo model doesn't have an IFormFile property (files come from HTTP multipart/form-data)
    /// - DTOs provide a clean separation between API contracts and database models
    /// - Makes Swagger documentation clearer
    /// 
    /// The [FromForm] attribute in the controller tells ASP.NET Core to bind this from multipart/form-data,
    /// which is the format browsers use when submitting files via HTML forms.
    /// </remarks>
    public class PhotoUploadDto
    {
        /// <summary>
        /// The ID of the vault this photo belongs to.
        /// Must match an existing Vault.Id in the database.
        /// </summary>
        public int VaultId { get; set; }

        /// <summary>
        /// The uploaded image file from the HTTP request.
        /// IFormFile is ASP.NET Core's representation of an uploaded file.
        /// It provides access to the file stream, filename, content type, and size.
        /// </summary>
        public IFormFile File { get; set; }
    }
}