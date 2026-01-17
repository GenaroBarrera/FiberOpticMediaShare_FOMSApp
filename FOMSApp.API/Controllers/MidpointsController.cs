using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FOMSApp.API.Data;
using FOMSApp.Shared.Models;

namespace FOMSApp.API.Controllers;

// API controller for midpoint CRUD operations.
[Route("api/[controller]")]
[ApiController]
public class MidpointsController(AppDbContext context, IWebHostEnvironment env, ILogger<MidpointsController> logger) : ControllerBase
{
    private readonly AppDbContext _context = context;
    private readonly IWebHostEnvironment _env = env;
    private readonly ILogger<MidpointsController> _logger = logger;

    // GET: api/midpoints - Gets all midpoints.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Midpoint>>> GetMidpoints()
    {
        return await _context.Midpoints
            .AsNoTracking()
            .ToListAsync();
    }

    // GET: api/midpoints/{id} - Gets a single midpoint by ID.
    [HttpGet("{id}")]
    public async Task<ActionResult<Midpoint>> GetMidpoint(int id)
    {
        var midpoint = await _context.Midpoints.FindAsync(id);

        if (midpoint == null)
            return NotFound();

        return midpoint;
    }

    // POST: api/midpoints - Creates a new midpoint.
    [HttpPost]
    public async Task<ActionResult<Midpoint>> PostMidpoint(Midpoint midpoint)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            _context.Midpoints.Add(midpoint);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created midpoint with ID: {MidpointId}", midpoint.Id);

            return CreatedAtAction(nameof(GetMidpoints), new { id = midpoint.Id }, midpoint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating midpoint");
            return StatusCode(500, "An error occurred while creating the midpoint.");
        }
    }

    // PUT: api/midpoints/{id} - Updates an existing midpoint.
    [HttpPut("{id}")]
    public async Task<IActionResult> PutMidpoint(int id, Midpoint midpoint)
    {
        if (id != midpoint.Id)
            return BadRequest("ID mismatch.");

        var existingMidpoint = await _context.Midpoints.FindAsync(id);
        if (existingMidpoint == null)
            return NotFound();

        existingMidpoint.Name = midpoint.Name;
        existingMidpoint.Status = midpoint.Status;
        existingMidpoint.Description = midpoint.Description;
        existingMidpoint.Location = midpoint.Location;
        existingMidpoint.Color = GetStatusColor(midpoint.Status);

        _context.Entry(existingMidpoint).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Midpoints.AnyAsync(e => e.Id == id))
                return NotFound();
            throw;
        }

        return NoContent();
    }

    // DELETE: api/midpoints/{id} - Deletes a midpoint and its associated photos.
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMidpoint(int id)
    {
        var midpoint = await _context.Midpoints
            .Include(m => m.Photos)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (midpoint == null)
            return NotFound();

        // Delete photo files from disk
        string uploadPath = Path.Combine(_env.WebRootPath, "uploads");
        foreach (var photo in midpoint.Photos)
        {
            string filePath = Path.Combine(uploadPath, photo.FileName);
            if (System.IO.File.Exists(filePath))
            {
                try { System.IO.File.Delete(filePath); }
                catch (Exception ex) 
                { 
                    _logger.LogWarning(ex, "Could not delete photo file: {FileName}", photo.FileName); 
                }
            }
        }

        _context.Midpoints.Remove(midpoint);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Maps midpoint status to marker color.
    private static string GetStatusColor(MidpointStatus status) => status switch
    {
        MidpointStatus.New => "Black",
        MidpointStatus.Review => "LightGray",
        MidpointStatus.Complete => "LightGreen",
        MidpointStatus.Issue => "LightCoral",
        _ => "Black"
    };
}
