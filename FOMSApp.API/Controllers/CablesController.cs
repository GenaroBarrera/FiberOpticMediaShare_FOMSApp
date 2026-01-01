using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FOMSApp.API.Data;
using FOMSApp.Shared.Models;

namespace FOMSApp.API.Controllers;

/// <summary>
/// API controller for cable route CRUD operations.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class CablesController(AppDbContext context) : ControllerBase
{
    private readonly AppDbContext _context = context;

    /// <summary>
    /// Gets all cable routes.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Cable>>> GetCables()
    {
        return await _context.Cables.ToListAsync();
    }

    /// <summary>
    /// Gets a single cable by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Cable>> GetCable(int id)
    {
        var cable = await _context.Cables.FindAsync(id);

        if (cable == null)
            return NotFound();

        return cable;
    }

    /// <summary>
    /// Creates a new cable route.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Cable>> PostCable(Cable cable)
    {
        _context.Cables.Add(cable);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCables), new { id = cable.Id }, cable);
    }

    /// <summary>
    /// Updates an existing cable route.
    /// </summary>
    [HttpPut("{id}")]
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

    /// <summary>
    /// Deletes a cable route.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCable(int id)
    {
        var cable = await _context.Cables.FindAsync(id);

        if (cable == null)
            return NotFound();

        _context.Cables.Remove(cable);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
