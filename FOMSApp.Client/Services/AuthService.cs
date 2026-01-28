using System.Net.Http.Json;

namespace FOMSApp.Client.Services;

/// <summary>
/// Service to manage authentication state by communicating with the API's auth endpoints.
/// Uses the BFF pattern - authentication is handled by the API via cookies.
/// </summary>
public class AuthService
{
    private readonly HttpClient _httpClient;
    private UserInfo? _cachedUser;
    private DateTime _lastFetch = DateTime.MinValue;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(1);

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Gets the current user info from the API.
    /// Results are cached for 1 minute to avoid excessive API calls.
    /// </summary>
    public async Task<UserInfo> GetCurrentUserAsync(bool forceRefresh = false)
    {
        // Return cached result if still valid
        if (!forceRefresh && _cachedUser != null && DateTime.UtcNow - _lastFetch < _cacheExpiry)
        {
            return _cachedUser;
        }

        try
        {
            var response = await _httpClient.GetFromJsonAsync<UserInfo>("api/auth/me");
            _cachedUser = response ?? new UserInfo();
            _lastFetch = DateTime.UtcNow;
            return _cachedUser;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching user info: {ex.Message}");
            _cachedUser = new UserInfo();
            _lastFetch = DateTime.UtcNow;
            return _cachedUser;
        }
    }

    /// <summary>
    /// Clears the cached user info. Call after login/logout.
    /// </summary>
    public void ClearCache()
    {
        _cachedUser = null;
        _lastFetch = DateTime.MinValue;
    }

    /// <summary>
    /// Gets the login URL. The API will redirect to Azure AD.
    /// </summary>
    public string GetLoginUrl(string? returnUrl = null)
    {
        var baseUrl = _httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "";
        var url = $"{baseUrl}/api/auth/login";
        if (!string.IsNullOrEmpty(returnUrl))
        {
            url += $"?returnUrl={Uri.EscapeDataString(returnUrl)}";
        }
        return url;
    }

    /// <summary>
    /// Gets the logout URL. The API will clear the cookie and sign out of Azure AD.
    /// </summary>
    public string GetLogoutUrl(string? returnUrl = null)
    {
        var baseUrl = _httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "";
        var url = $"{baseUrl}/api/auth/logout";
        if (!string.IsNullOrEmpty(returnUrl))
        {
            url += $"?returnUrl={Uri.EscapeDataString(returnUrl)}";
        }
        return url;
    }
}

/// <summary>
/// User information returned by the auth service.
/// Matches the API's UserInfo class.
/// </summary>
public class UserInfo
{
    public bool IsAuthenticated { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public List<string> Roles { get; set; } = new();

    // Convenience properties for role checking
    public bool IsAdmin => Roles.Contains("Admin");
    public bool IsEditor => Roles.Contains("Admin") || Roles.Contains("Editor");
    public bool IsViewer => Roles.Contains("Admin") || Roles.Contains("Editor") || Roles.Contains("Viewer");
}
