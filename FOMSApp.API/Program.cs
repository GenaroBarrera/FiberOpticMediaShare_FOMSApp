using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using FOMSApp.API.Data;
using FOMSApp.API.Services;
using FOMSApp.API.Configuration;
using NetTopologySuite.IO.Converters;
using System.Text.Json.Serialization;
using System.Security.Claims;
using Azure.Storage.Blobs;

var builder = WebApplication.CreateBuilder(args);

// Configure JSON serialization
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new GeoJsonConverterFactory());
    });

// Configure file upload limits (20MB max)
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 20 * 1024 * 1024;
    options.ValueLengthLimit = int.MaxValue;
    options.ValueCountLimit = int.MaxValue;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure cookie-based Azure AD authentication (BFF pattern)
var azureAdSection = builder.Configuration.GetSection("AzureAd");
var clientUrl = builder.Configuration["ClientUrl"] ?? "https://fomsapp-client-dev-h8gpfta0hybueaeu.centralus-01.azurewebsites.net";

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.Name = "FOMSApp.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.None; // Required for cross-origin requests
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    
    // Return 401 for API calls instead of redirecting
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = 403;
        return Task.CompletedTask;
    };
})
.AddOpenIdConnect(options =>
{
    options.Authority = $"{azureAdSection["Instance"]}{azureAdSection["TenantId"]}/v2.0";
    options.ClientId = azureAdSection["ClientId"];
    options.ClientSecret = azureAdSection["ClientSecret"];
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    
    // Map the roles claim from Azure AD
    options.TokenValidationParameters.RoleClaimType = "roles";
    
    options.Events.OnRedirectToIdentityProvider = context =>
    {
        // Store the return URL for after login
        if (context.Properties.RedirectUri != null)
        {
            context.Properties.Items["returnUrl"] = context.Properties.RedirectUri;
        }
        return Task.CompletedTask;
    };
    
    options.Events.OnTokenValidated = context =>
    {
        // Log successful authentication
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        var userName = context.Principal?.Identity?.Name ?? "Unknown";
        var roles = context.Principal?.Claims.Where(c => c.Type == "roles").Select(c => c.Value).ToList() ?? [];
        logger.LogInformation("User {User} authenticated with roles: {Roles}", userName, string.Join(", ", roles));
        
        // Transform Azure AD "roles" claims to standard ClaimTypes.Role for authorization policies
        if (context.Principal?.Identity is ClaimsIdentity identity)
        {
            var existingRoleClaims = identity.Claims.Where(c => c.Type == "roles").ToList();
            foreach (var roleClaim in existingRoleClaims)
            {
                // Add as standard role claim type so RequireRole() works
                identity.AddClaim(new Claim(ClaimTypes.Role, roleClaim.Value));
            }
        }
        
        return Task.CompletedTask;
    };
});

// Register claims transformation to map Azure AD roles to standard role claims
builder.Services.AddTransient<IClaimsTransformation, RoleClaimsTransformation>();

// Configure authorization policies - check both "roles" claim (Azure AD) and standard Role claim
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireEditor", policy => 
        policy.RequireAssertion(context =>
        {
            var roles = context.User.Claims
                .Where(c => c.Type == "roles" || c.Type == ClaimTypes.Role)
                .Select(c => c.Value);
            return roles.Any(r => r == "Admin" || r == "Editor");
        }))
    .AddPolicy("RequireAdmin", policy => 
        policy.RequireAssertion(context =>
        {
            var roles = context.User.Claims
                .Where(c => c.Type == "roles" || c.Type == ClaimTypes.Role)
                .Select(c => c.Value);
            return roles.Any(r => r == "Admin");
        }));

// Configure database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, x => x.UseNetTopologySuite()));

// Maintenance services
builder.Services.AddScoped<DeletedEntitiesPurger>();
builder.Services.AddHostedService<DeletedEntitiesPurgeHostedService>();

// Configure storage service based on configuration
var storageOptions = builder.Configuration.GetSection(StorageOptions.SectionName).Get<StorageOptions>() 
    ?? new StorageOptions { Provider = "Local" };
builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection(StorageOptions.SectionName));

// Register storage service based on provider
if (storageOptions.Provider.Equals("Azure", StringComparison.OrdinalIgnoreCase))
{
    if (string.IsNullOrWhiteSpace(storageOptions.AzureConnectionString))
    {
        throw new InvalidOperationException(
            "Azure storage is configured but AzureConnectionString is missing. " +
            "Please set Storage:AzureConnectionString in appsettings.json or environment variables.");
    }

    builder.Services.AddSingleton(sp => new BlobServiceClient(storageOptions.AzureConnectionString));
    builder.Services.AddSingleton<IStorageService>(sp =>
    {
        var blobServiceClient = sp.GetRequiredService<BlobServiceClient>();
        var logger = sp.GetRequiredService<ILogger<AzureBlobStorageService>>();
        return new AzureBlobStorageService(
            blobServiceClient,
            storageOptions.AzureContainerName,
            logger,
            storageOptions.AzureBaseUrl);
    });
}
else
{
    // Default to local file storage
    builder.Services.AddSingleton<IStorageService, LocalFileStorageService>();
}

// Configure CORS - must allow credentials for cookie auth
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        policy.WithOrigins(
                clientUrl,
                "https://localhost:7166", // Local client HTTPS
                "http://localhost:5173"   // Local client HTTP
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Required for cookies
    });
});

var app = builder.Build();

// CORS must be early in the pipeline, before other middleware
app.UseCors("AllowClient");

// Global exception handler to ensure proper error responses
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Unhandled exception in request pipeline");
        
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { 
                error = "Internal server error", 
                message = ex.Message,
                type = ex.GetType().Name
            });
        }
    }
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

// Root endpoint - redirect to Swagger in development, or return API info
app.MapGet("/", () => 
{
    if (app.Environment.IsDevelopment())
    {
        return Results.Redirect("/swagger");
    }
    return Results.Json(new { 
        message = "FOMSApp API is running",
        endpoints = new[] { 
            "/api/vaults", 
            "/api/cables", 
            "/api/midpoints", 
            "/api/photos" 
        }
    });
});

app.MapControllers();

app.Run();

//do a quick commit test
//do another quick commit test
