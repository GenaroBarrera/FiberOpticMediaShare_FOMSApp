using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FOMSApp.API.Data;
using FOMSApp.Shared.Models;

namespace FOMSApp.API.Controllers;

// API controller for vault CRUD operations.
[Route("api/[controller]")]
[ApiController]
public class VaultsController(AppDbContext context, IWebHostEnvironment env, ILogger<VaultsController> logger) : ControllerBase
{
    private readonly AppDbContext _context = context;
    private readonly IWebHostEnvironment _env = env;
    private readonly ILogger<VaultsController> _logger = logger;

    // GET: api/vaults - Gets all vaults with their associated photos.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Vault>>> GetVaults()
    {
        return await _context.Vaults
            .Where(v => !v.IsDeleted)
            .Include(v => v.Photos)
            .AsNoTracking()
            .ToListAsync();
    }

    // GET: api/vaults/{id} - Gets a single vault by ID.
    [HttpGet("{id}")]
    public async Task<ActionResult<Vault>> GetVault(int id)
    {
        var vault = await _context.Vaults.FindAsync(id);

        if (vault == null)
            return NotFound();

        return vault;
    }

    // POST: api/vaults - Creates a new vault.
    [HttpPost]
    public async Task<ActionResult<Vault>> PostVault(Vault vault)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            _context.Vaults.Add(vault);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created vault with ID: {VaultId}", vault.Id);

            return CreatedAtAction(nameof(GetVault), new { id = vault.Id }, vault);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vault");
            return StatusCode(500, "An error occurred while creating the vault.");
        }
    }

    // PUT: api/vaults/{id} - Updates an existing vault.
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
        existingVault.IsDeleted = vault.IsDeleted;
        existingVault.DeletedAt = vault.IsDeleted
            ? (existingVault.DeletedAt ?? DateTimeOffset.UtcNow)
            : null;

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

    // DELETE: api/vaults/{id} - Deletes a vault and its associated photos.
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVault(int id)
    {
        var vault = await _context.Vaults
            .Include(v => v.Photos)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vault == null)
            return NotFound();

        // Soft delete:
        // - Preserve the DB row so it can be restored via Undo.
        // - Preserve photo rows/files so downloads/history aren't destroyed.
        vault.IsDeleted = true;
        vault.DeletedAt ??= DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Maps vault status to marker color.
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
