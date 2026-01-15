# Local Development Setup Guide

This guide sets up FOMSApp for **completely local development** - no Azure services required, **100% free**.

## Prerequisites

- .NET 9.0 SDK
- Visual Studio 2022 or VS Code
- SQL Server LocalDB (comes with Visual Studio) OR SQLite

## Step 1: Database Setup

### Option A: SQL Server LocalDB (Recommended)

SQL Server LocalDB is included with Visual Studio and is free.

**Connection String** (`appsettings.json`):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FOMSDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Option B: SQLite (Simplest)

SQLite requires no installation and works everywhere.

**Connection String** (`appsettings.json`):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=fomsapp.db"
  }
}
```

**Note**: For SQLite, you'll need to add the SQLite provider package:
```bash
dotnet add FOMSApp.API package Microsoft.EntityFrameworkCore.Sqlite
```

Then update `Program.cs` to use SQLite when connection string contains `.db`:
```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (connectionString?.Contains(".db") == true)
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(connectionString, x => x.UseNetTopologySuite()));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString, x => x.UseNetTopologySuite()));
}
```

## Step 2: Configure Local Storage

Photos will be stored locally in `wwwroot/uploads` folder.

**No configuration needed** - the `BlobStorageService` automatically uses local storage when Azure connection string is not set.

## Step 3: Disable Azure AD Authentication

Leave Azure AD settings empty in `appsettings.json`:
```json
{
  "AzureAd": {
    "Instance": "",
    "Domain": "",
    "TenantId": "",
    "ClientId": "",
    "CallbackPath": "",
    "Audience": ""
  }
}
```

The app will run without authentication (perfect for local development).

## Step 4: Update Client Configuration

Update `FOMSApp.Client/Program.cs`:
```csharp
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri("http://localhost:5083") 
});
```

## Step 5: Run Database Migrations

```bash
cd FOMSApp.API
dotnet ef database update
```

## Step 6: Run the Application

### Run API:
```bash
cd FOMSApp.API
dotnet run
```

API will be available at: `http://localhost:5083`

### Run Client:
```bash
cd FOMSApp.Client
dotnet run
```

Client will be available at: `http://localhost:5000` (or port shown in console)

## Step 7: Verify Everything Works

1. Open `http://localhost:5083/swagger` - API should be accessible
2. Open `http://localhost:5000` - Client should load
3. Test creating a vault, uploading photos, etc.

## Local Development Features

✅ **Database**: SQL Server LocalDB or SQLite (free)
✅ **Storage**: Local file system (`wwwroot/uploads`)
✅ **Authentication**: Disabled (or mock)
✅ **Hosting**: Kestrel/IIS Express (free)
✅ **Cost**: **$0/month**

## File Structure

```
FOMSApp/
├── FOMSApp.API/
│   ├── wwwroot/
│   │   └── uploads/          ← Photos stored here locally
│   └── fomsapp.db            ← SQLite database (if using SQLite)
├── FOMSApp.Client/
└── FOMSApp.Shared/
```

## Troubleshooting

### Database Connection Issues

**SQL Server LocalDB**:
- Ensure LocalDB is installed: `sqllocaldb info`
- Start LocalDB: `sqllocaldb start mssqllocaldb`

**SQLite**:
- Ensure `Microsoft.EntityFrameworkCore.Sqlite` package is installed
- Check that `Program.cs` uses SQLite provider

### Photo Upload Issues

- Ensure `wwwroot/uploads` folder exists
- Check file permissions
- Verify file size limits (20MB max)

### CORS Issues

- CORS is configured to allow all origins in development
- Check that API is running on correct port

## Migrating to Azure Later

When ready to deploy to Azure:

1. **Database**: Update connection string to Azure SQL
2. **Storage**: Set Azure Blob Storage connection string
3. **Authentication**: Configure Azure AD (optional)
4. **Deploy**: Use GitHub Actions workflow

**No code changes needed** - just update configuration!

## Benefits of Local Development

- ✅ **Instant feedback** - No network latency
- ✅ **Free** - No Azure costs
- ✅ **Fast** - No cold starts
- ✅ **Offline** - Works without internet
- ✅ **Debugging** - Full debugging support
- ✅ **Privacy** - Data stays local

## Next Steps

1. Set up local development environment (this guide)
2. Develop and test features locally
3. When ready, deploy to Azure free tier for testing
4. Finally, deploy to paid tier for production
