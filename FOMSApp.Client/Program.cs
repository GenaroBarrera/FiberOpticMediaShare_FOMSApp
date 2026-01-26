using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;
using FOMSApp.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// API base URL comes from client config (wwwroot/appsettings*.json).
// This lets launch profiles/environment switch API targets without code changes.
var apiBaseUrl = builder.Configuration["ApiBaseUrl"];
if (string.IsNullOrWhiteSpace(apiBaseUrl))
{
    apiBaseUrl = "https://localhost:7165/";
}
apiBaseUrl = apiBaseUrl.TrimEnd('/') + "/";

// Configure MSAL authentication
builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    var apiScope = builder.Configuration["ApiScope"];
    if (!string.IsNullOrWhiteSpace(apiScope))
    {
        options.ProviderOptions.DefaultAccessTokenScopes.Add(apiScope);
    }
});

// Configure HttpClient with authentication for API calls
builder.Services.AddScoped<CustomAuthorizationMessageHandler>();

builder.Services.AddHttpClient("FOMSApp.API", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.AddHttpMessageHandler<CustomAuthorizationMessageHandler>();

// Register the authenticated HttpClient as the default
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("FOMSApp.API"));

await builder.Build().RunAsync();

// Custom handler that attaches access tokens to API requests
public class CustomAuthorizationMessageHandler : AuthorizationMessageHandler
{
    public CustomAuthorizationMessageHandler(IAccessTokenProvider provider, NavigationManager navigation, IConfiguration configuration)
        : base(provider, navigation)
    {
        var apiBaseUrl = configuration["ApiBaseUrl"]?.TrimEnd('/') + "/";
        ConfigureHandler(authorizedUrls: new[] { apiBaseUrl ?? "https://localhost:7165/" });
    }
}