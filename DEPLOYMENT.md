# Azure Deployment Guide for FOMSApp (Production Setup)

This guide will help you deploy FOMSApp to Azure with **production-grade services** (~$28-30/month).

## 💰 Cost-Saving Alternatives

**Looking for free options?** Check out:
- 🆓 **`LOCAL_DEVELOPMENT_SETUP.md`** - 100% free local development (no Azure)
- 🆓 **`FREE_TIER_DEPLOYMENT.md`** - Azure free tier setup ($0/month for 12 months)
- 📊 **`COST_OPTIMIZED_SETUP.md`** - Compare all options and costs

## Prerequisites

## Prerequisites

1. Azure subscription
2. Azure CLI installed
3. GitHub repository with your code

## Step 1: Create Azure Resources

### 1.1 Create Resource Group
```bash
az group create --name FOMSApp-RG --location eastus
```

### 1.2 Create Azure SQL Database
```bash
# Create SQL Server
az sql server create \
  --name fomsapp-sql-server \
  --resource-group FOMSApp-RG \
  --location eastus \
  --admin-user <your-admin-username> \
  --admin-password <your-strong-password>

# Create SQL Database
az sql db create \
  --resource-group FOMSApp-RG \
  --server fomsapp-sql-server \
  --name FOMSDb \
  --service-objective S0 \
  --backup-storage-redundancy Local

# Allow Azure services to access SQL Server
az sql server firewall-rule create \
  --resource-group FOMSApp-RG \
  --server fomsapp-sql-server \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

### 1.3 Create Azure Storage Account (for Blob Storage)
```bash
az storage account create \
  --name fomsappstorage \
  --resource-group FOMSApp-RG \
  --location eastus \
  --sku Standard_LRS

# Create blob container
az storage container create \
  --name photos \
  --account-name fomsappstorage \
  --public-access blob
```

### 1.4 Create Azure App Service Plan
```bash
az appservice plan create \
  --name fomsapp-plan \
  --resource-group FOMSApp-RG \
  --sku B1 \
  --is-linux
```

### 1.5 Create Azure Web App
```bash
az webapp create \
  --name fomsapp-api \
  --resource-group FOMSApp-RG \
  --plan fomsapp-plan \
  --runtime "DOTNET|9.0"
```

## Step 2: Configure App Settings

### 2.1 Get Connection Strings
```bash
# Get SQL connection string
az sql db show-connection-string \
  --server fomsapp-sql-server \
  --name FOMSDb \
  --client ado.net

# Get Storage connection string
az storage account show-connection-string \
  --name fomsappstorage \
  --resource-group FOMSApp-RG
```

### 2.2 Set App Settings in Azure Portal
Go to Azure Portal → Your Web App → Configuration → Application settings and add:

- `ConnectionStrings:DefaultConnection` = Your SQL connection string
- `Azure:Storage:ConnectionString` = Your Storage connection string
- `Azure:Storage:ContainerName` = `photos`
- `AzureAd:TenantId` = Your Azure AD Tenant ID
- `AzureAd:ClientId` = Your Azure AD App Registration Client ID
- `AzureAd:Instance` = `https://login.microsoftonline.com/`
- `AzureAd:Domain` = Your Azure AD domain
- `AzureAd:Audience` = Your Azure AD App ID URI
- `AllowedOrigins` = Your client app URL (comma-separated if multiple)

## Step 3: Configure Azure AD

### 3.1 Register App in Azure AD
1. Go to Azure Portal → Azure Active Directory → App registrations
2. Click "New registration"
3. Name: `FOMSApp API`
4. Supported account types: Choose based on your needs
5. Redirect URI: Leave blank for API
6. Click "Register"

### 3.2 Configure API Permissions
1. In your app registration, go to "Expose an API"
2. Set Application ID URI (e.g., `api://<client-id>`)
3. Add scopes if needed

### 3.3 Update appsettings.json with Azure AD values
- TenantId: Found in Overview page
- ClientId: Found in Overview page
- Domain: Your Azure AD domain
- Audience: The Application ID URI you set

## Step 4: Configure GitHub Actions

### 4.1 Get Publish Profile
1. Go to Azure Portal → Your Web App → Get publish profile
2. Download the `.PublishSettings` file

### 4.2 Add GitHub Secret
1. Go to your GitHub repository
2. Settings → Secrets and variables → Actions
3. Click "New repository secret"
4. Name: `AZURE_WEBAPP_PUBLISH_PROFILE`
5. Value: Copy the entire contents of the `.PublishSettings` file
6. Click "Add secret"

### 4.3 Update Workflow
Edit `.github/workflows/azure-deploy.yml` and set:
- `AZURE_WEBAPP_NAME` to your actual Azure Web App name

## Step 5: Run Database Migrations

After deployment, run Entity Framework migrations:

```bash
# Option 1: Using Azure Cloud Shell
az webapp ssh --name fomsapp-api --resource-group FOMSApp-RG

# Then run:
cd /home/site/wwwroot
dotnet ef database update --project FOMSApp.API

# Option 2: From your local machine (if you have the connection string)
dotnet ef database update --project FOMSApp.API --connection "YOUR_CONNECTION_STRING"
```

## Step 6: Verify Deployment

1. Check the GitHub Actions tab to see if deployment succeeded
2. Visit `https://<your-app-name>.azurewebsites.net/swagger` to test the API
3. Test API endpoints to ensure everything works

## Troubleshooting

### Connection Issues
- Verify connection strings are correct
- Check SQL Server firewall rules
- Ensure "Allow Azure services" is enabled

### Authentication Issues
- Verify Azure AD configuration
- Check that CORS is properly configured
- Ensure client app is registered in Azure AD

### Blob Storage Issues
- Verify storage account connection string
- Check container exists and has proper permissions
- Ensure container name matches configuration
