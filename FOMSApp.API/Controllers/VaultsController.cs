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
    }
}