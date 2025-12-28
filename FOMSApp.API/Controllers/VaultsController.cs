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

        /// <summary>
        /// Updates an existing vault's properties (name, status, description).
        /// Does not update the Location property - vaults cannot be moved after creation.
        /// </summary>
        /// <param name="id">The unique ID of the vault to update (from the URL route)</param>
        /// <param name="vault">
        /// The updated vault data. Only Name, Status, and Description properties are updated.
        /// The Location property is ignored to prevent accidental movement of vaults.
        /// </param>
        /// <returns>
        /// HTTP 204 No Content if successfully updated, HTTP 400 Bad Request if the ID doesn't match,
        /// or HTTP 404 Not Found if the vault doesn't exist.
        /// </returns>
        /// <remarks>
        /// RESTful Best Practice: PUT operations typically return 204 No Content (success with no body)
        /// when the update is successful, as the client already knows what was sent.
        /// 
        /// This method uses a partial update pattern - only the provided properties are updated.
        /// The Location property is explicitly preserved to prevent vaults from being moved.
        /// 
        /// Entity Framework's Update() method marks all properties as modified, so we manually
        /// set only the properties we want to allow editing.
        /// </remarks>
        // PUT: api/vaults/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVault(int id, Vault vault)
        {
            // Ensure the ID in the URL matches the ID in the request body
            if (id != vault.Id)
            {
                return BadRequest("The ID in the URL does not match the ID in the request body.");
            }

            // Find the existing vault in the database
            var existingVault = await _context.Vaults.FindAsync(id);
            if (existingVault == null)
            {
                return NotFound();
            }

            // Update only the editable properties (preserve Location and other system properties)
            existingVault.Name = vault.Name;
            existingVault.Status = vault.Status;
            existingVault.Description = vault.Description;
            
            // Update Color property to match the new Status
            existingVault.Color = GetStatusColor(vault.Status);
            
            // Note: Location is NOT updated - vaults cannot be moved after creation

            // Mark the entity as modified so Entity Framework knows to update it
            _context.Entry(existingVault).State = EntityState.Modified;

            try
            {
                // Execute the SQL UPDATE statement
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Handle concurrency conflicts (if another user modified the vault simultaneously)
                if (!await _context.Vaults.AnyAsync(e => e.Id == id))
                {
                    return NotFound();
                }
                throw; // Re-throw if it's a different concurrency issue
            }

            // Return 204 No Content (successful update with no response body)
            return NoContent();
        }

        /// <summary>
        /// Deletes a vault from the database by its unique identifier.
        /// Also deletes all associated photos (cascade delete handled by database or explicit deletion).
        /// </summary>
        /// <param name="id">The unique ID of the vault to delete (from the URL route)</param>
        /// <returns>
        /// HTTP 204 No Content if successfully deleted, or HTTP 404 Not Found if the vault doesn't exist.
        /// </returns>
        /// <remarks>
        /// RESTful Best Practice: DELETE operations typically return 204 No Content (success with no body)
        /// rather than 200 OK, because there's nothing meaningful to return after deletion.
        /// 
        /// Note: This will also delete all photos associated with the vault if cascade delete is configured.
        /// If not, you may need to explicitly delete photos first to avoid foreign key constraint violations.
        /// </remarks>
        // DELETE: api/vaults/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVault(int id)
        {
            // Find the vault by primary key
            var vault = await _context.Vaults.FindAsync(id);
            
            // Return 404 if not found (standard REST practice)
            if (vault == null)
                return NotFound();

            // Mark the entity for deletion
            _context.Vaults.Remove(vault);
            
            // Execute the SQL DELETE statement
            await _context.SaveChangesAsync();
            
            // Return 204 No Content (successful deletion with no response body)
            return NoContent();
        }

        /// <summary>
        /// Helper method to get the color name string based on the vault status.
        /// Maps status colors: New (blue), Pending (brown), Review (gray), Complete (green), Issue (red).
        /// </summary>
        /// <param name="status">The vault status to get the color for</param>
        /// <returns>A color name string that matches the status marker color</returns>
        private static string GetStatusColor(VaultStatus status)
        {
            return status switch
            {
                VaultStatus.New => "Blue",
                VaultStatus.Pending => "Brown",
                VaultStatus.Review => "Gray",
                VaultStatus.Complete => "Green",
                VaultStatus.Issue => "Red",
                _ => "Blue" // Default to blue
            };
        }
    }
}