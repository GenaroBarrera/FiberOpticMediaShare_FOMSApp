using Microsoft.EntityFrameworkCore;
using FOMSApp.API.Data;
using NetTopologySuite.IO.Converters; // <--- Critical for Map Data
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 1. Add services to the container.

// --- FIX: Configure JSON Options for Swagger & API ---
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Prevent "Cycle" errors (loops in data)
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        
        // Teach the API how to read/write GeoJSON (Points, Lines)
        options.JsonSerializerOptions.Converters.Add(new GeoJsonConverterFactory());
    });
// -----------------------------------------------------

// Configure form options to allow larger file uploads (up to 20MB)
// This is necessary because ASP.NET Core has default limits on multipart form body size
// The default limit is typically 128MB for Kestrel, but form parsing has stricter limits
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    // Set the maximum multipart form body size to 20MB (20 * 1024 * 1024 bytes)
    // This allows construction photos to be uploaded without hitting size limits
    // Note: This applies to the entire multipart form body, including all fields and files
    options.MultipartBodyLengthLimit = 20 * 1024 * 1024; // 20MB
    
    // Increase value length limit to allow larger form field values
    options.ValueLengthLimit = int.MaxValue; // No limit on individual form field values
    
    // Increase value count limit (number of form fields)
    options.ValueCountLimit = int.MaxValue; // No limit on number of form fields
});

// Configure form options to allow larger file uploads (up to 20MB)
// This is necessary because ASP.NET Core has default limits on request body size (typically 30MB for Kestrel, but form limits are lower)
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    // Set the maximum multipart form body size to 20MB (20 * 1024 * 1024 bytes)
    // This allows construction photos to be uploaded without hitting size limits
    // Note: Individual files can be up to 20MB, and the total form body can be up to 20MB
    options.MultipartBodyLengthLimit = 20 * 1024 * 1024; // 20MB
    
    // Increase value length limit to allow larger form field values
    options.ValueLengthLimit = int.MaxValue; // No limit on individual form field values
    
    // Increase value count limit (number of form fields)
    options.ValueCountLimit = int.MaxValue; // No limit on number of form fields
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. Configure the Database (SQL Server)
builder.Configuration.AddJsonFile("appsettings.json");
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, x => x.UseNetTopologySuite())); // Ensure NTS is enabled for SQL

// 3. Enable CORS (Cross-Origin Requests) - Good practice for Blazor
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

var app = builder.Build();

// 4. Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Keep commented out if running strictly on HTTP/5083

app.UseCors("AllowAll");

// ENABLE STATIC FILES (For your photo uploads)
app.UseStaticFiles(); 

app.UseAuthorization();

app.MapControllers();

app.Run();