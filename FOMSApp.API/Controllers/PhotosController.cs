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
        public async Task<ActionResult<Photo>> UploadPhoto([FromForm] IFormFile file, [FromForm] int vaultId)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // 1. Create the "Uploads" folder if it doesn't exist
            // We save inside wwwroot so the browser can access it later
            string uploadPath = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // 2. Generate a unique filename (to prevent overwrites)
            // e.g., "b123-guid-5678.jpg"
            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string fullPath = Path.Combine(uploadPath, uniqueFileName);

            // 3. Save the file to the hard drive
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 4. Save the record in the Database
            var photo = new Photo
            {
                FileName = uniqueFileName,
                VaultId = vaultId,
                UploadedAt = DateTime.Now
            };

            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            return Ok(photo);
        }
    }
}