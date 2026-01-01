using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FOMSApp.API.Data;
using FOMSApp.Shared.Models;

namespace FOMSApp.API.Controllers;

/// <summary>
/// API controller for vault CRUD operations.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class VaultsController(AppDbContext context, IWebHostEnvironment env) : ControllerBase
{
    private readonly AppDbContext _context = context;
    private readonly IWebHostEnvironment _env = env;

    /// <summary>
    /// Gets all vaults with their associated photos.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Vault>>> GetVaults()
    {
        return await _context.Vaults
            .Include(v => v.Photos)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a single vault by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Vault>> GetVault(int id)
    {
        var vault = await _context.Vaults.FindAsync(id);

        if (vault == null)
            return NotFound();

        return vault;
    }

    /// <summary>
    /// Creates a new vault.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Vault>> PostVault(Vault vault)
    {
        _context.Vaults.Add(vault);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetVault), new { id = vault.Id }, vault);
    }

    /// <summary>
    /// Updates an existing vault.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> PutVault(int id, Vault vault)
    {
        if (id != vault.Id)
            return BadRequest("ID mismatch.");

        var existingVault = await _context.Vaults.FindAsync(id);
        if (existingVault == null)
            return NotFound();

        existingVault.Name = vault.Name;
        existingVault.Status = vault.Status;
        existingVault.Description = vault.Description;
        existingVault.Location = vault.Location;
        existingVault.Color = GetStatusColor(vault.Status);

        _context.Entry(existingVault).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Vaults.AnyAsync(e => e.Id == id))
                return NotFound();
            throw;
        }

        return NoContent();
    }

    /// <summary>
    /// Deletes a vault and its associated photos.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVault(int id)
    {
        var vault = await _context.Vaults
            .Include(v => v.Photos)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vault == null)
            return NotFound();

        // Delete photo files from disk
        string uploadPath = Path.Combine(_env.WebRootPath, "uploads");
        foreach (var photo in vault.Photos)
        {
            string filePath = Path.Combine(uploadPath, photo.FileName);
            if (System.IO.File.Exists(filePath))
            {
                try { System.IO.File.Delete(filePath); }
                catch (Exception ex) { Console.WriteLine($"Warning: Could not delete {photo.FileName}: {ex.Message}"); }
            }
        }

        _context.Vaults.Remove(vault);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Maps vault status to marker color.
    /// </summary>
    private static string GetStatusColor(VaultStatus status) => status switch
    {
        VaultStatus.New => "Blue",
        VaultStatus.Pending => "Brown",
        VaultStatus.Review => "Gray",
        VaultStatus.Complete => "Green",
        VaultStatus.Issue => "Red",
        _ => "Blue"
    };
}
