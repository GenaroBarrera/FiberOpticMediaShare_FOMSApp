# FOMSApp - Fiber Optic Management System

A web-based construction management tool for fiber optic network installations. FOMSApp combines geospatial visualization with photo management, enabling real-time QA status tracking on an interactive map.

## Features

- **Interactive Map** - Leaflet.js map with OpenStreetMap and satellite imagery
- **Infrastructure Tracking** - Manage vaults, midpoints, and cable routes with drag-drop positioning
- **Photo Documentation** - Upload and attach photos to infrastructure points
- **Status Workflow** - Color-coded markers track progress (New, Pending, Review, Complete, Issue)
- **Address Search** - Find locations via geocoding
- **Bulk Operations** - Multi-select for batch photo downloads

## Tech Stack

| Layer | Technology |
|-------|------------|
| Runtime | .NET 9 / C# 13 |
| Backend | ASP.NET Core Web API |
| Frontend | Blazor WebAssembly |
| Database | SQL Server with EF Core |
| Spatial | NetTopologySuite |
| Mapping | Leaflet.js |
| UI | Bootstrap 5 |

## Architecture

```
FOMSApp/
├── FOMSApp.API/          # ASP.NET Core Web API (Controllers, EF Core, Migrations)
├── FOMSApp.Client/       # Blazor WebAssembly SPA (Pages, Leaflet.js integration)
└── FOMSApp.Shared/       # Shared models and enums
```

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Express, Developer, or LocalDB)

### Setup

1. Clone the repository
   ```bash
   git clone https://github.com/GenaroBarrera/FiberOpticMediaShare_FOMSApp.git
   cd FOMSApp
   ```

2. Configure the database connection in `FOMSApp.API/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=FOMSDb;Trusted_Connection=True;TrustServerCertificate=True;"
     }
   }
   ```

3. Apply database migrations
   ```bash
   cd FOMSApp.API
   dotnet ef database update
   ```

4. Run the API (Terminal 1)
   ```bash
   cd FOMSApp.API
   dotnet run
   ```

5. Run the Client (Terminal 2)
   ```bash
   cd FOMSApp.Client
   dotnet run
   ```

6. Open `http://localhost:5187` in your browser

API documentation (Swagger) is available at `http://localhost:5083/swagger`.

## Roadmap

- Azure SQL Database and App Service hosting
- Azure Blob Storage for photos
- Azure AD authentication
- CI/CD with GitHub Actions
- Mobile app with .NET MAUI

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
