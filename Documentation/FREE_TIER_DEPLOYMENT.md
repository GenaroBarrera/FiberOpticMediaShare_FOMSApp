# Free Tier Azure Deployment Guide

This guide deploys FOMSApp to Azure using **only free tier services** (or minimal cost).

## Prerequisites

1. Azure account (free account with $200 credit available)
2. Azure CLI installed
3. GitHub repository

## Step 1: Create Azure Resources (Free Tier)

### 1.1 Create Resource Group
```bash
az group create --name FOMSApp-Free-RG --location eastus
```

### 1.2 Create Azure App Service Plan (FREE Tier)
```bash
az appservice plan create \
  --name fomsapp-free-plan \
  --resource-group FOMSApp-Free-RG \
  --sku FREE \
  --is-linux
```

**Note**: FREE tier limitations:
- 1 GB storage
- Shared CPU
- 60 minutes/day compute time
- No custom domains
- No SSL certificates

### 1.3 Create Azure Web App
```bash
az webapp create \
  --name fomsapp-api-free \
  --resource-group FOMSApp-Free-RG \
  --plan fomsapp-free-plan \
  --runtime "DOTNET|9.0"
```

### 1.4 Create Azure Storage Account (Free Tier)
```bash
az storage account create \
  --name fomsappstoragefree \
  --resource-group FOMSApp-Free-RG \
  --location eastus \
  --sku Standard_LRS \
  --kind StorageV2

# Create blob container
az storage container create \
  --name photos \
  --account-name fomsappstoragefree \
  --public-access blob
```

**Note**: First 12 months includes:
- 5 GB LRS storage
- 20,000 read operations/month
- 10,000 write operations/month

### 1.5 Use SQLite Instead of Azure SQL (FREE)

For free tier setup, we'll use SQLite instead of Azure SQL Database.

**Cost**: $0/month

## Step 2: Configure Application for SQLite

### 2.1 Update appsettings.json for SQLite

Add SQLite connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=fomsapp.db"
  }
}
```

### 2.2 Update Program.cs to Support SQLite

The app will automatically use SQLite if the connection string points to a `.db` file.

## Step 3: Configure App Settings

### 3.1 Get Storage Connection String
```bash
az storage account show-connection-string \
  --name fomsappstoragefree \
  --resource-group FOMSApp-Free-RG \
  --query connectionString \
  --output tsv
```

### 3.2 Set App Settings in Azure Portal

Go to Azure Portal → Your Web App → Configuration → Application settings:

- `ConnectionStrings:DefaultConnection` = `Data Source=/home/site/wwwroot/fomsapp.db`
- `Azure:Storage:ConnectionString` = Your Storage connection string
- `Azure:Storage:ContainerName` = `photos`

**Note**: Azure AD is optional - leave blank for free setup without authentication.

## Step 4: Deploy Database (SQLite)

SQLite database file will be created automatically on first run, or you can:

1. Create database locally
2. Upload `fomsapp.db` to `/home/site/wwwroot/` via FTP/Kudu

### Access Kudu Console
```
https://fomsapp-api-free.scm.azurewebsites.net
```

## Step 5: Configure GitHub Actions (Free)

### 5.1 Get Publish Profile
1. Azure Portal → Your Web App → Get publish profile
2. Download `.PublishSettings` file

### 5.2 Add GitHub Secret
1. GitHub → Settings → Secrets → Actions
2. Add secret: `AZURE_WEBAPP_PUBLISH_PROFILE`
3. Paste publish profile contents

### 5.3 Update Workflow
Edit `.github/workflows/azure-deploy.yml`:
```yaml
env:
  AZURE_WEBAPP_NAME: fomsapp-api-free
```

## Step 6: Limitations & Workarounds

### FREE Tier Limitations:

1. **App Service Free Tier**:
   - ⚠️ 60 minutes/day compute time
   - ⚠️ App sleeps after inactivity
   - ✅ Workaround: Use "Always On" is not available, but app wakes on first request

2. **Storage Free Tier**:
   - ⚠️ 5 GB limit (first 12 months)
   - ✅ Workaround: Delete old photos or compress images

3. **SQLite**:
   - ⚠️ Single file database
   - ⚠️ No concurrent writes
   - ✅ Workaround: Fine for low-traffic apps

### Recommended for:
- ✅ Development/testing
- ✅ Demos and prototypes
- ✅ Low-traffic applications
- ✅ Learning Azure

### Not Recommended for:
- ❌ Production workloads
- ❌ High-traffic applications
- ❌ Applications requiring 24/7 uptime

## Step 7: Verify Deployment

1. Check GitHub Actions deployment
2. Visit `https://fomsapp-api-free.azurewebsites.net/swagger`
3. Test API endpoints

## Cost Breakdown

| Service | Cost |
|---------|------|
| App Service (FREE) | $0/month |
| Storage (Free Tier) | $0/month (first 12 months) |
| SQLite | $0/month |
| Azure AD (Free) | $0/month |
| **Total** | **$0/month** |

After 12 months: ~$0.50/month (storage only)

## Upgrading Later

When ready to upgrade:

1. **Database**: Migrate SQLite → Azure SQL Database
2. **App Service**: Upgrade FREE → B1 (~$13/month)
3. **Storage**: Continues on free tier, then Hot tier (~$0.50/month)

No code changes needed - just update connection strings!
