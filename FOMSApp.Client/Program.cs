using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using FOMSApp.Client;
using FOMSApp.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// API base URL comes from client config (wwwroot/appsettings*.json).
var apiBaseUrl = builder.Configuration["ApiBaseUrl"];
if (string.IsNullOrWhiteSpace(apiBaseUrl))
{
    apiBaseUrl = "https://localhost:7165/";
}
apiBaseUrl = apiBaseUrl.TrimEnd('/') + "/";

// Configure HttpClient with credentials handler for cookie authentication
builder.Services.AddScoped<CookieHandler>();
builder.Services.AddHttpClient("FOMSApp.API", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.AddHttpMessageHandler<CookieHandler>();

// Register HttpClient as the default for injection
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("FOMSApp.API"));

// Register AuthService for authentication state management
builder.Services.AddScoped<AuthService>();

await builder.Build().RunAsync();

/// <summary>
/// HTTP message handler that includes credentials (cookies) with all requests.
/// Required for the BFF authentication pattern to work with cross-origin API.
/// </summary>
public class CookieHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Include credentials (cookies) with the request
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        
        // Ensure CORS preflight requests include credentials header
        request.Headers.Add("X-Requested-With", "XMLHttpRequest");
        
        return await base.SendAsync(request, cancellationToken);
    }
}