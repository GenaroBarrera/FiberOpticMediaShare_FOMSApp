using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using FOMSApp.API.Data;
using FOMSApp.API.Services;
using NetTopologySuite.IO.Converters;
using System.Text.Json.Serialization;

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

// Register Azure Blob Storage service
builder.Services.AddScoped<BlobStorageService>();

// Configure database
builder.Configuration.AddJsonFile("appsettings.json");
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Support both SQL Server and SQLite (for local development)
if (!string.IsNullOrWhiteSpace(connectionString) && connectionString.Contains(".db"))
{
    // SQLite for local development (free, no Azure needed)
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(connectionString, x => x.UseNetTopologySuite()));
}
else if (!string.IsNullOrWhiteSpace(connectionString))
{
    // SQL Server (LocalDB or Azure SQL)
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString, x => x.UseNetTopologySuite()));
}

// Configure Azure AD Authentication (optional - only if configured)
var tenantId = builder.Configuration["AzureAd:TenantId"];
var clientId = builder.Configuration["AzureAd:ClientId"];

if (!string.IsNullOrWhiteSpace(tenantId) && !string.IsNullOrWhiteSpace(clientId))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
    
    builder.Services.AddAuthorization();
}

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    
    // CORS policy for authenticated requests (when Azure AD is configured)
    if (!string.IsNullOrWhiteSpace(tenantId) && !string.IsNullOrWhiteSpace(clientId))
    {
        options.AddPolicy("AllowAuthenticated", policy =>
        {
            policy.WithOrigins(builder.Configuration["AllowedOrigins"]?.Split(',') ?? Array.Empty<string>())
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    }
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseStaticFiles();

// Use authentication and authorization if Azure AD is configured
if (!string.IsNullOrWhiteSpace(tenantId) && !string.IsNullOrWhiteSpace(clientId))
{
    app.UseAuthentication();
    app.UseAuthorization();
}

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

