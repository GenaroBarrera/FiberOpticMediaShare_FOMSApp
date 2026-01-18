# Azure Integration Setup Guide

This document explains how to configure the FOMSApp to use Azure services or fall back to local development.

## Storage Service Configuration

The application uses an abstraction layer for file storage that supports both local file system and Azure Blob Storage. The storage provider is configured via `appsettings.json`.

### Local Storage (Default - Development)

For local development, the application uses the local file system. No additional configuration is needed.

**appsettings.Development.json:**
```json
{
  "Storage": {
    "Provider": "Local"
  }
}
```

Files are stored in `wwwroot/uploads/` directory.

### Azure Blob Storage (Production)

To use Azure Blob Storage, configure the following settings:

**appsettings.Production.json or appsettings.json:**
```json
{
  "Storage": {
    "Provider": "Azure",
    "AzureConnectionString": "DefaultEndpointsProtocol=https;AccountName=yourstorageaccount;AccountKey=yourkey;EndpointSuffix=core.windows.net",
    "AzureContainerName": "photos",
    "AzureBaseUrl": ""
  }
}
```

#### Setting up Azure Blob Storage:

1. **Create a Storage Account in Azure Portal:**
   - Go to Azure Portal → Create a resource → Storage Account
   - Choose a unique name, select a resource group, and region
   - Review and create

2. **Get the Connection String:**
   - Navigate to your Storage Account
   - Go to "Access keys" in the left menu
   - Copy "Connection string" from key1 or key2

3. **Create a Container:**
   - In your Storage Account, go to "Containers" in the left menu
   - Click "+ Container"
   - Name it "photos" (or match `AzureContainerName` in config)
   - Set Public access level to "Blob" (for public access) or "Private" (for private access)

4. **Configure the Application:**
   - Add the connection string to `appsettings.Production.json` or set it as an environment variable
   - Set `Storage:Provider` to `"Azure"`

### Environment Variables (Recommended for Production)

For security, use environment variables instead of storing connection strings in appsettings:

**Azure App Service Configuration:**
- Go to your App Service → Configuration → Application settings
- Add:
  - `Storage:Provider` = `Azure`
  - `Storage:AzureConnectionString` = `[your connection string]`
  - `Storage:AzureContainerName` = `photos`

**Local Development (PowerShell):**
```powershell
$env:Storage__Provider = "Azure"
$env:Storage__AzureConnectionString = "DefaultEndpointsProtocol=https;AccountName=..."
$env:Storage__AzureContainerName = "photos"
```

## Database Configuration

### Local SQL Server (Default - Development)

**appsettings.Development.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=FOMSDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Azure SQL Database (Production)

**appsettings.Production.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:yourserver.database.windows.net,1433;Initial Catalog=FOMSDb;Persist Security Info=False;User ID=yourusername;Password=yourpassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

#### Setting up Azure SQL Database:

1. **Create SQL Database in Azure Portal:**
   - Go to Azure Portal → Create a resource → SQL Database
   - Choose server (create new if needed), database name, and pricing tier
   - Review and create

2. **Configure Firewall:**
   - Go to your SQL Server → Networking
   - Add your IP address or enable "Allow Azure services and resources to access this server"

3. **Get Connection String:**
   - Go to your SQL Database → Connection strings
   - Copy the ADO.NET connection string
   - Replace `{your_username}` and `{your_password}` with actual credentials

4. **Configure the Application:**
   - Add the connection string to `appsettings.Production.json` or set as environment variable

## Switching Between Local and Azure

The application automatically detects the storage provider based on configuration:

- **Development Environment:** Uses `appsettings.Development.json` → defaults to Local storage
- **Production Environment:** Uses `appsettings.Production.json` → can use Azure storage

To switch manually, change the `Storage:Provider` value:
- `"Local"` → Uses local file system
- `"Azure"` → Uses Azure Blob Storage (requires connection string)

## Migration Notes

### Migrating Existing Photos to Azure Blob Storage

If you have existing photos in local storage and want to migrate to Azure:

1. Ensure Azure Blob Storage is configured
2. Create a migration script or use Azure Storage Explorer to upload files
3. The file names in the database should match the blob names in Azure

### Fallback Behavior

- If Azure is configured but connection fails, the application will throw an error on startup
- If Local is configured, it will always work (assuming file system permissions)

## Troubleshooting

### "AzureConnectionString is missing" Error

- Ensure `Storage:Provider` is set to `"Azure"` in your configuration
- Verify the connection string is correctly formatted
- Check that environment variables are set if using them

### Photos Not Uploading to Azure

- Verify the container exists and is accessible
- Check the connection string is correct
- Ensure the Storage Account allows the required operations
- Check application logs for detailed error messages

### Local Storage Not Working

- Ensure `wwwroot/uploads/` directory exists and is writable
- Check file system permissions
- Verify `Storage:Provider` is set to `"Local"` or not specified (defaults to Local)
