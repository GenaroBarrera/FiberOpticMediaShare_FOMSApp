using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FOMSApp.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// POINT THIS TO YOUR API PORT (Check your terminal from Phase 1, likely 5083 or 5143)
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5083") });

await builder.Build().RunAsync();
