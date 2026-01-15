# .NET MAUI Mobile App Setup

This document provides instructions for setting up and completing the .NET MAUI mobile app for FOMSApp.

## Prerequisites

1. Visual Studio 2022 with .NET MAUI workload installed
2. .NET 9.0 SDK
3. Android SDK (for Android development)
4. Xcode (for iOS development, macOS only)

## Initial Setup

### 1. Install .NET MAUI Workload

```bash
dotnet workload install maui
```

### 2. Restore NuGet Packages

```bash
cd FOMSApp.Mobile
dotnet restore
```

### 3. Add Project to Solution

```bash
cd ..
dotnet sln add FOMSApp.Mobile/FOMSApp.Mobile.csproj
```

## Project Structure

The MAUI project includes:

- **Services/**: API service layer for communicating with the backend
- **ViewModels/**: MVVM view models (to be created)
- **Views/**: XAML pages (to be created)
- **Resources/**: Images, fonts, and other resources

## Next Steps

### 1. Create AppShell

Create `AppShell.xaml` and `AppShell.xaml.cs` for navigation.

### 2. Create ViewModels

Implement MVVM pattern with view models for:
- VaultsViewModel
- VaultDetailsViewModel
- MidpointsViewModel
- CablesViewModel

### 3. Create Views

Create XAML pages for:
- VaultsPage (list view)
- VaultDetailsPage (detail view with map)
- MidpointsPage (list view)
- CablesPage (list view)

### 4. Add Map Integration

Integrate a map control (e.g., Mapbox, Google Maps) to display vaults, midpoints, and cables.

### 5. Configure API URL

Update `ApiService.cs` with your Azure App Service URL:
```csharp
private const string BaseUrl = "https://your-app.azurewebsites.net";
```

### 6. Add Authentication

If using Azure AD authentication, integrate Microsoft.Identity.Client for mobile authentication.

## Building and Running

### Android
```bash
dotnet build -t:Run -f net9.0-android
```

### iOS (macOS only)
```bash
dotnet build -t:Run -f net9.0-ios
```

### Windows
```bash
dotnet build -t:Run -f net9.0-windows10.0.19041.0
```

## Resources

- [.NET MAUI Documentation](https://learn.microsoft.com/dotnet/maui/)
- [.NET MAUI Samples](https://github.com/dotnet/maui-samples)
