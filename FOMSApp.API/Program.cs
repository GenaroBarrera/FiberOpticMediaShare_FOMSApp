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