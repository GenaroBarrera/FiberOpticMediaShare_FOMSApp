using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FOMSApp.API.Data;
using FOMSApp.Shared.Models;

namespace FOMSApp.API.Controllers
{
    /// <summary>
    /// RESTful API controller for managing Vault entities.
    /// Handles HTTP requests for creating, reading, updating, and deleting vault records.
    /// </summary>
    /// <remarks>
    /// ASP.NET Core automatically converts these methods to HTTP endpoints:
    /// - GET /api/vaults - Get all vaults
    /// - GET /api/vaults/{id} - Get a specific vault
    /// - POST /api/vaults - Create a new vault
    /// 
    /// The [Route] attribute means URLs start with /api/vaults (controller name minus "Controller")
    /// </remarks>
    [Route("api/[controller]")] // This makes the route /api/vaults (removes "Controller" from class name)
    [ApiController] // Enables automatic model validation and other API-specific features
    public class VaultsController : ControllerBase
    {
        /// <summary>
        /// Database context for accessing vault data. Injected via constructor dependency injection.
        /// 
        /// Best Practice: Marked as 'readonly' to prevent reassignment after construction.
        /// This ensures the context remains consistent throughout the controller's lifetime.
        /// </summary>
        private readonly AppDbContext _context;

        /// <summary>
        /// Constructor that receives the database context via dependency injection.
        /// 
        /// Dependency Injection Pattern: ASP.NET Core automatically provides an AppDbContext
        /// instance when creating this controller. This makes testing easier and ensures
        /// proper lifetime management (the framework handles disposal).
        /// </summary>
        /// <param name="context">The database context instance provided by the framework</param>
        public VaultsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all vaults from the database, including their associated photos.
        /// </summary>
        /// <returns>
        /// HTTP 200 OK with a list of all vaults, or HTTP 500 if a database error occurs.
        /// Each vault includes its Photos collection due to the .Include() method.
        /// </returns>
        /// <remarks>
        /// Best Practice: Using .Include() is called "eager loading" - it loads related data
        /// in a single database query instead of making separate queries for each vault's photos.
        /// This is more efficient than "lazy loading" (loading photos on-demand).
        /// 
        /// The method is async because database operations are I/O-bound (waiting for disk/network),
        /// so using async/await doesn't block threads, improving server scalability.
        /// </remarks>
        // GET: api/Vaults
        [HttpGet] // Maps to HTTP GET requests
        public async Task<ActionResult<IEnumerable<Vault>>> GetVaults()
        {
            // Eager loading: Fetch vaults AND their photos in one database query
            // Without .Include(), the Photos list would be empty (lazy loading would require additional queries)
            return await _context.Vaults
                .Include(v => v.Photos) // Include related Photo entities in the query
                .ToListAsync(); // Execute the query asynchronously and return a List
        }

        /// <summary>
        /// Retrieves a single vault by its unique identifier.
        /// </summary>
        /// <param name="id">The unique ID of the vault to retrieve (from the URL route)</param>
        /// <returns>
        /// HTTP 200 OK with the vault if found, or HTTP 404 Not Found if the vault doesn't exist.
        /// </returns>
        /// <remarks>
        /// FindAsync() is optimized for looking up a single record by primary key.
        /// It's faster than using .Where() or .FirstOrDefaultAsync() for primary key lookups.
        /// 
        /// Note: This method doesn't include Photos. If you need photos, use GetVaults() and filter,
        /// or modify this method to include .Include(v => v.Photos).
        /// </remarks>
        // GET: api/vaults/5
        [HttpGet("{id}")] // The {id} is a route parameter extracted from the URL
        public async Task<ActionResult<Vault>> GetVault(int id)
        {
            // FindAsync is optimized for primary key lookups (very fast)
            var vault = await _context.Vaults.FindAsync(id);

            // Return 404 Not Found if the vault doesn't exist (standard REST practice)
            if (vault == null)
            {
                return NotFound(); // Returns HTTP 404 status code
            }

            return vault; // Returns HTTP 200 OK with the vault data
        }

        /// <summary>
        /// Creates a new vault in the database.
        /// </summary>
        /// <param name="vault">
        /// The vault object to create. ASP.NET Core automatically deserializes JSON from the request body
        /// into this Vault object. The Location property must be provided (it's marked as 'required').
        /// </param>
        /// <returns>
        /// HTTP 201 Created with the newly created vault (including its auto-generated ID),
        /// along with a Location header pointing to the new resource.
        /// Returns HTTP 400 Bad Request if model validation fails.
        /// </returns>
        /// <remarks>
        /// RESTful Best Practice: CreatedAtAction() returns:
        /// 1. HTTP 201 (Created) status code (not 200 OK)
        /// 2. A Location header with the URL to retrieve the new vault: /api/vaults/{id}
        /// 3. The created vault in the response body
        /// 
        /// This allows clients to immediately know where to find the newly created resource.
        /// </remarks>
        // POST: api/vaults
        [HttpPost] // Maps to HTTP POST requests
        public async Task<ActionResult<Vault>> PostVault(Vault vault)
        {
            // Add the vault to the DbContext's change tracker (marks it as "to be inserted")
            _context.Vaults.Add(vault);
            
            // SaveChangesAsync() executes the SQL INSERT statement
            // After this, vault.Id will contain the auto-generated primary key value
            await _context.SaveChangesAsync();
            
            // Return 201 Created with a Location header pointing to the new resource
            // nameof(GetVault) prevents typos - the compiler ensures the method name is correct
            return CreatedAtAction(nameof(GetVault), new { id = vault.Id }, vault);
        }
    }
}