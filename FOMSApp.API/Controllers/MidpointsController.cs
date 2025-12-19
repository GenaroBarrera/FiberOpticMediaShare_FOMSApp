using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FOMSApp.API.Data;
using FOMSApp.Shared.Models;

namespace FOMSApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MidpointsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MidpointsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/midpoints
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Midpoint>>> GetMidpoints()
        {
            return await _context.Midpoints.ToListAsync();
        }

        // POST: api/midpoints
        [HttpPost]
        public async Task<ActionResult<Midpoint>> PostMidpoint(Midpoint midpoint)
        {
            _context.Midpoints.Add(midpoint);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetMidpoints), new { id = midpoint.Id }, midpoint);
        }
    }
}