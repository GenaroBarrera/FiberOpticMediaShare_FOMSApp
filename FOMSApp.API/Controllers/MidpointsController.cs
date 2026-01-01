using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FOMSApp.API.Data;
using FOMSApp.Shared.Models;

namespace FOMSApp.API.Controllers;

/// <summary>
/// API controller for midpoint CRUD operations.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class MidpointsController(AppDbContext context, IWebHostEnvironment env) : ControllerBase
{
    private readonly AppDbContext _context = context;
    private readonly IWebHostEnvironment _env = env;

    /// <summary>
    /// Gets all midpoints.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Midpoint>>> GetMidpoints()
    {
        return await _context.Midpoints.ToListAsync();
    }

    /// <summary>
    /// Gets a single midpoint by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Midpoint>> GetMidpoint(int id)
    {
        var midpoint = await _context.Midpoints.FindAsync(id);

        if (midpoint == null)
            return NotFound();

        return midpoint;
    }

    /// <summary>
    /// Creates a new midpoint.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Midpoint>> PostMidpoint(Midpoint midpoint)
    {
        _context.Midpoints.Add(midpoint);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMidpoints), new { id = midpoint.Id }, midpoint);
    }

    /// <summary>
    /// Updates an existing midpoint.
    /// </summary>
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

    /// <summary>
    /// Deletes a midpoint and its associated photos.
    /// </summary>
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
                catch (Exception ex) { Console.WriteLine($"Warning: Could not delete {photo.FileName}: {ex.Message}"); }
            }
        }

        _context.Midpoints.Remove(midpoint);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Maps midpoint status to marker color.
    /// </summary>
    private static string GetStatusColor(MidpointStatus status) => status switch
    {
        MidpointStatus.New => "Black",
        MidpointStatus.Review => "LightGray",
        MidpointStatus.Complete => "LightGreen",
        MidpointStatus.Issue => "LightCoral",
        _ => "Black"
    };
}
