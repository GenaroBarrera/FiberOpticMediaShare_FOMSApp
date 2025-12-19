using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FOMSApp.API.Data;
using FOMSApp.Shared.Models;

namespace FOMSApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CablesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CablesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/cables
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cable>>> GetCables()
        {
            return await _context.Cables.ToListAsync();
        }

        // POST: api/cables
        [HttpPost]
        public async Task<ActionResult<Cable>> PostCable(Cable cable)
        {
            _context.Cables.Add(cable);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCables), new { id = cable.Id }, cable);
        }

        // DELETE: api/cables/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCable(int id)
        {
            var cable = await _context.Cables.FindAsync(id);
            if (cable == null) return NotFound();

            _context.Cables.Remove(cable);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}