using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FOMSApp.API.Data;
using FOMSApp.Shared.Models;

namespace FOMSApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VaultsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VaultsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Vaults
        [HttpGet] 
        public async Task<ActionResult<IEnumerable<Vault>>> GetVaults()
        {
            // We use .Include to fetch the Photos along with the Vault
            return await _context.Vaults
                .Include(v => v.Photos)
                .ToListAsync();
        }

        // GET: api/vaults/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Vault>> GetVault(int id)
        {
            var vault = await _context.Vaults.FindAsync(id);

            if (vault == null)
            {
                return NotFound();
            }

            return vault;
        }

        // POST: api/vaults
        /// <summary>
        /// Creates a new vault at the specified location with the provided name and status.
        /// </summary>
        /// <param name="vault">The vault object containing location, name, color, and status.</param>
        /// <returns>The created vault with its assigned ID.</returns>
        [HttpPost]
        public async Task<ActionResult<Vault>> PostVault(Vault vault)
        {
            _context.Vaults.Add(vault);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetVault), new { id = vault.Id }, vault);
        }
    }
}