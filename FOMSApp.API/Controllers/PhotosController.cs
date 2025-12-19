using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FOMSApp.API.Data;
using FOMSApp.Shared.Models;

namespace FOMSApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PhotosController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/photos/vault/5
        // This gets all photos for a specific vault
        [HttpGet("vault/{vaultId}")]
        public async Task<ActionResult<IEnumerable<Photo>>> GetPhotosForVault(int vaultId)
        {
            return await _context.Photos
                .Where(p => p.VaultId == vaultId)
                .ToListAsync();
        }

        // POST: api/photos
        // This receives the file and saves it
        [HttpPost]
        // POST: api/photos
        [HttpPost]
        public async Task<ActionResult<Photo>> UploadPhoto([FromForm] PhotoUploadDto upload)
        {
            // Update logic to use 'upload.File' and 'upload.VaultId'
            if (upload.File == null || upload.File.Length == 0)
                return BadRequest("No file uploaded.");

            string uploadPath = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(upload.File.FileName);
            string fullPath = Path.Combine(uploadPath, uniqueFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await upload.File.CopyToAsync(stream);
            }

            var photo = new Photo
            {
                FileName = uniqueFileName,
                VaultId = upload.VaultId, // usage updated
                UploadedAt = DateTime.Now
            };

            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            return Ok(photo);
        }
    }
}

// Wraps the file and the ID together so Swagger is happy
public class PhotoUploadDto
{
    public int VaultId { get; set; }
    public IFormFile File { get; set; }
}