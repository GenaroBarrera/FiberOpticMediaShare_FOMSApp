using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FOMSApp.API.Data;
using FOMSApp.Shared.Models;

namespace FOMSApp.API.Controllers;

// API controller for cable route CRUD operations.
[Route("api/[controller]")]
[ApiController]
public class CablesController(AppDbContext context, ILogger<CablesController> logger) : ControllerBase
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<CablesController> _logger = logger;

    // GET: api/cables - Gets all cable routes.
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Cable>>> GetCables()
    {
        return await _context.Cables
            .Where(c => !c.IsDeleted)
            .AsNoTracking()
            .ToListAsync();
    }

    // GET: api/cables/{id} - Gets a single cable by ID.
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<Cable>> GetCable(int id)
    {
        var cable = await _context.Cables.FindAsync(id);

        if (cable == null)
            return NotFound();

        return cable;
    }

    // POST: api/cables - Creates a new cable route.
    [HttpPost]
    [Authorize(Policy = "RequireEditor")]
    public async Task<ActionResult<Cable>> PostCable(Cable cable)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            _context.Cables.Add(cable);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created cable with ID: {CableId}", cable.Id);

            return CreatedAtAction(nameof(GetCables), new { id = cable.Id }, cable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating cable");
            return StatusCode(500, "An error occurred while creating the cable.");
        }
    }

    // PUT: api/cables/{id} - Updates an existing cable route.
    [HttpPut("{id}")]
    [Authorize(Policy = "RequireEditor")]
    public async Task<IActionResult> PutCable(int id, Cable cable)
    {
        if (id != cable.Id)
            return BadRequest("ID mismatch.");

        var existingCable = await _context.Cables.FindAsync(id);
        if (existingCable == null)
            return NotFound();

        existingCable.Name = cable.Name;
        existingCable.Description = cable.Description;
        existingCable.Color = cable.Color;
        existingCable.Path = cable.Path;
        existingCable.IsDeleted = cable.IsDeleted;
        existingCable.DeletedAt = cable.IsDeleted
            ? (existingCable.DeletedAt ?? DateTimeOffset.UtcNow)
            : null;

        _context.Entry(existingCable).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Cables.AnyAsync(c => c.Id == id))
                return NotFound();
            throw;
        }

        return NoContent();
    }

    // DELETE: api/cables/{id} - Deletes a cable route.
    [HttpDelete("{id}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> DeleteCable(int id)
    {
        var cable = await _context.Cables.FindAsync(id);

        if (cable == null)
            return NotFound();

        // Soft delete (preserve row so Undo can restore).
        cable.IsDeleted = true;
        cable.DeletedAt ??= DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
