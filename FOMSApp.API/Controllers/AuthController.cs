using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FOMSApp.API.Controllers;

/// <summary>
/// Authentication controller for BFF (Backend-for-Frontend) pattern.
/// Handles login/logout via Azure AD and provides user info to the client.
/// </summary>
[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Initiates Azure AD login. Redirects to Azure AD, then back to client after successful auth.
    /// </summary>
    [HttpGet("login")]
    public IActionResult Login([FromQuery] string? returnUrl = null)
    {
        var clientUrl = _configuration["ClientUrl"] ?? "https://fomsapp-client-dev-h8gpfta0hybueaeu.centralus-01.azurewebsites.net";
        
        // Validate returnUrl to prevent open redirect attacks
        if (string.IsNullOrEmpty(returnUrl) || !returnUrl.StartsWith(clientUrl))
        {
            returnUrl = clientUrl;
        }

        _logger.LogInformation("Login initiated, will redirect to: {ReturnUrl}", returnUrl);

        return Challenge(new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(LoginCallback), new { returnUrl })
        }, OpenIdConnectDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Callback after successful Azure AD authentication. Redirects back to the client.
    /// </summary>
    [HttpGet("login-callback")]
    [Authorize]
    public IActionResult LoginCallback([FromQuery] string? returnUrl = null)
    {
        var clientUrl = _configuration["ClientUrl"] ?? "https://fomsapp-client-dev-h8gpfta0hybueaeu.centralus-01.azurewebsites.net";
        
        if (string.IsNullOrEmpty(returnUrl) || !returnUrl.StartsWith(clientUrl))
        {
            returnUrl = clientUrl;
        }

        _logger.LogInformation("Login callback for user {User}, redirecting to: {ReturnUrl}", 
            User.Identity?.Name, returnUrl);

        return Redirect(returnUrl);
    }

    /// <summary>
    /// Logs out the user by clearing the auth cookie.
    /// Uses a simple local logout (clears cookie) and redirects to client.
    /// </summary>
    [HttpGet("logout")]
    public async Task<IActionResult> Logout([FromQuery] string? returnUrl = null)
    {
        var clientUrl = _configuration["ClientUrl"] ?? "https://fomsapp-client-dev-h8gpfta0hybueaeu.centralus-01.azurewebsites.net";
        
        if (string.IsNullOrEmpty(returnUrl) || !returnUrl.StartsWith(clientUrl))
        {
            returnUrl = clientUrl;
        }

        _logger.LogInformation("Logout initiated for user {User}", User.Identity?.Name);

        // Clear the local auth cookie (simple logout without Azure AD redirect)
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        
        // Redirect directly to the client
        return Redirect(returnUrl);
    }

    /// <summary>
    /// Debug endpoint to see all claims. Remove in production.
    /// </summary>
    [HttpGet("claims")]
    [AllowAnonymous]
    public IActionResult GetClaims()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Ok(new { isAuthenticated = false, claims = Array.Empty<object>() });
        }

        var claims = User.Claims.Select(c => new { type = c.Type, value = c.Value }).ToList();
        return Ok(new { isAuthenticated = true, claimCount = claims.Count, claims });
    }

    /// <summary>
    /// Returns the current user's information and roles.
    /// Used by the client to check authentication state.
    /// </summary>
    [HttpGet("me")]
    [AllowAnonymous]
    public IActionResult GetCurrentUser()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Ok(new UserInfo
            {
                IsAuthenticated = false,
                Name = null,
                Email = null,
                Roles = []
            });
        }

        var roles = User.Claims
            .Where(c => c.Type == "roles" || c.Type == System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .Distinct()
            .ToList();

        var email = User.Claims.FirstOrDefault(c => 
            c.Type == "preferred_username" || 
            c.Type == "email" || 
            c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;

        var name = User.Claims.FirstOrDefault(c => 
            c.Type == "name" || 
            c.Type == System.Security.Claims.ClaimTypes.Name)?.Value 
            ?? User.Identity.Name;

        _logger.LogDebug("GetCurrentUser: {Name}, Roles: {Roles}", name, string.Join(", ", roles));

        return Ok(new UserInfo
        {
            IsAuthenticated = true,
            Name = name,
            Email = email,
            Roles = roles
        });
    }
}

/// <summary>
/// User information returned by the /api/auth/me endpoint.
/// </summary>
public class UserInfo
{
    public bool IsAuthenticated { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public List<string> Roles { get; set; } = [];
    
    // Convenience properties for role checking
    public bool IsAdmin => Roles.Contains("Admin");
    public bool IsEditor => Roles.Contains("Admin") || Roles.Contains("Editor");
    public bool IsViewer => Roles.Contains("Admin") || Roles.Contains("Editor") || Roles.Contains("Viewer");
}
