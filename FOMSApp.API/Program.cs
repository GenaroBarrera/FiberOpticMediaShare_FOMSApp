using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using FOMSApp.API.Data;
using FOMSApp.API.Services;
using FOMSApp.API.Configuration;
using NetTopologySuite.IO.Converters;
using System.Text.Json.Serialization;
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

// Configure Azure AD authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

// Configure authorization policies
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireEditor", policy => 
        policy.RequireRole("Admin", "Editor"))
    .AddPolicy("RequireAdmin", policy => 
        policy.RequireRole("Admin"));

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

// Configure CORS
builder.Services.AddCors(options =>
{
    // Development: Allow all origins for local development
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    
    // Production: Use specific origins (configure in appsettings.json)
    options.AddPolicy("Production", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
            ?? Array.Empty<string>();
        
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});

var app = builder.Build();

// CORS must be early in the pipeline, before other middleware
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
}
else
{
    app.UseCors("Production");
}

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
