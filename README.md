# FOMSApp - Fiber Optic Management System

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat&logo=dotnet)
![Blazor](https://img.shields.io/badge/Blazor-WebAssembly-512BD4?style=flat&logo=blazor)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Database-CC2927?style=flat&logo=microsoftsqlserver)
![License](https://img.shields.io/badge/License-MIT-green?style=flat)

**FOMSApp** is a web-based construction management tool designed to optimize the Quality Assurance (QA) workflow for fiber optic network installations. It provides an interactive map interface for tracking vaults, midpoints, and cable routes, with integrated photo documentation capabilities.

## 🎯 Problem Statement

Field crews installing fiber optic infrastructure need to document their work with photos at specific GPS locations. Project coordinators must review these photos and track installation progress across dozens or hundreds of sites. Traditional methods involve:

- Manual photo organization in folders
- Spreadsheets to track vault locations and status
- Email chains for photo submission and review
- No visual representation of the network topology

**FOMSApp solves this** by combining geospatial visualization with photo management, enabling real-time status tracking on an interactive map.

---

## Key Features

### Current Implementation ✅

| Feature | Description |
|---------|-------------|
| **Interactive Map** | Leaflet.js-powered map with OpenStreetMap and Esri Satellite imagery layers |
| **Vault Management** | Create, edit, delete, and drag-drop reposition vault markers on the map |
| **Midpoint Markers** | Track intermediate points (slack loops, splice points) along cable routes |
| **Cable Drawing** | Draw polyline cable routes connecting infrastructure points |
| **Photo Uploads** | Attach multiple photos to vaults and midpoints (up to 20MB per upload) |
| **Status Workflow** | Visual status tracking with color-coded markers (New → Pending → Review → Complete/Issue) |
| **Address Search** | Geocoding via OpenStreetMap Nominatim to find locations by address |
| **Multi-Select** | Select multiple vaults/midpoints for bulk photo downloads |
| **Undo Functionality** | Revert recent map editing actions |
| **Cascade Delete** | Automatically removes associated photos when deleting vaults/midpoints |

### Status Color Legend

| Status | Vault Color | Midpoint Color | Meaning |
|--------|-------------|----------------|---------|
| New | 🔵 Blue | ⚫ Black | Just created, no action taken |
| Pending | 🟤 Brown | — | Waiting for field crew photos |
| Review | ⚪ Gray | 🔘 Light Gray | Photos uploaded, awaiting coordinator review |
| Complete | 🟢 Green | 🟢 Light Green | Approved and finalized |
| Issue | 🔴 Red | 🔴 Light Red | Problem identified, needs attention |

---

## 🛠️ Tech Stack

### Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        FOMSApp Solution                         │
├─────────────────┬─────────────────────┬─────────────────────────┤
│  FOMSApp.API    │  FOMSApp.Client     │  FOMSApp.Shared         │
│  (Backend)      │  (Frontend)         │  (Common Models)        │
├─────────────────┼─────────────────────┼─────────────────────────┤
│ ASP.NET Core    │ Blazor WebAssembly  │ Entity Classes          │
│ Web API         │ (C# in browser)     │ Enums                   │
│ EF Core         │ Leaflet.js Maps     │ Shared DTOs             │
│ SQL Server      │ Bootstrap CSS       │                         │
└─────────────────┴─────────────────────┴─────────────────────────┘
```

### Technology Details

| Layer | Technology | Version | Purpose |
|-------|------------|---------|---------|
| **Runtime** | .NET | 9.0 | Cross-platform framework |
| **Language** | C# | 13 | Primary development language |
| **Backend** | ASP.NET Core Web API | 9.0 | RESTful HTTP API |
| **Frontend** | Blazor WebAssembly | 9.0 | Single-page application (SPA) |
| **ORM** | Entity Framework Core | 9.0.0 | Database access and migrations |
| **Database** | SQL Server | — | Relational data storage |
| **Spatial** | NetTopologySuite | 2.6.0 | GeoJSON and spatial data types |
| **Mapping** | Leaflet.js | 1.9.x | Interactive map rendering |
| **UI Framework** | Bootstrap | 5.x | Responsive CSS styling |
| **API Docs** | Swashbuckle (Swagger) | 6.6.2 | OpenAPI documentation |

### Database Schema

```
┌─────────────────┐       ┌─────────────────┐
│     Vaults      │       │     Photos      │
├─────────────────┤       ├─────────────────┤
│ Id (PK)         │──┐    │ Id (PK)         │
│ Name            │  │    │ FileName        │
│ Color           │  │    │ UploadedAt      │
│ Status          │  └───▶│ VaultId (FK)    │
│ Description     │       │ MidpointId (FK) │
│ Location (Geo)  │  ┌───▶│                 │
└─────────────────┘  │    └─────────────────┘
                     │
┌─────────────────┐  │    ┌─────────────────┐
│   Midpoints     │──┘    │     Cables      │
├─────────────────┤       ├─────────────────┤
│ Id (PK)         │       │ Id (PK)         │
│ Name            │       │ Name            │
│ Color           │       │ Color           │
│ Status          │       │ Description     │
│ Description     │       │ Path (Geo)      │
│ Location (Geo)  │       └─────────────────┘
└─────────────────┘
```

---

## 📁 Project Structure

```
FOMSApp/
├── FOMSApp.sln                    # Visual Studio solution file
├── README.md                      # This file
├── LICENSE                        # MIT License
│
├── FOMSApp.API/                   # Backend Web API
│   ├── Controllers/               # REST API endpoints
│   │   ├── VaultsController.cs
│   │   ├── MidpointsController.cs
│   │   ├── CablesController.cs
│   │   └── PhotosController.cs
│   ├── Data/
│   │   ├── AppDbContext.cs        # EF Core database context
│   │   └── DbInitializer.cs       # Seed data (if applicable)
│   ├── Migrations/                # EF Core database migrations
│   ├── wwwroot/uploads/           # Uploaded photo storage
│   ├── Program.cs                 # Application entry point
│   └── appsettings.json           # Configuration
│
├── FOMSApp.Client/                # Blazor WebAssembly Frontend
│   ├── Pages/
│   │   ├── Home.razor             # Main map interface
│   │   ├── VaultDetails.razor     # Vault detail/edit page
│   │   ├── MidpointDetails.razor  # Midpoint detail/edit page
│   │   └── CableDetails.razor     # Cable detail/edit page
│   ├── Layout/
│   │   ├── MainLayout.razor       # Application layout
│   │   └── NavMenu.razor          # Navigation sidebar
│   ├── wwwroot/
│   │   ├── css/app.css            # Custom styles
│   │   ├── mapHelper.js           # Leaflet.js interop functions
│   │   └── index.html             # SPA entry point
│   └── Program.cs                 # Blazor configuration
│
└── FOMSApp.Shared/                # Shared Class Library
    └── Models/
        ├── Vault.cs               # Vault entity
        ├── Midpoint.cs            # Midpoint entity
        ├── Cable.cs               # Cable entity
        ├── Photo.cs               # Photo entity
        ├── VaultStatus.cs         # Status enum
        └── MidpointStatus.cs      # Status enum
```

---

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Express, Developer, or LocalDB)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/) with C# extension

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/YourUsername/FOMSApp.git
   cd FOMSApp
   ```

2. **Configure the database connection**
   
   Update `FOMSApp.API/appsettings.json` with your SQL Server connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=FOMSDb;Trusted_Connection=True;TrustServerCertificate=True;"
     }
   }
   ```

3. **Apply database migrations**
   ```bash
   cd FOMSApp.API
   dotnet ef database update
   ```

4. **Run the API** (Terminal 1)
   ```bash
   cd FOMSApp.API
   dotnet run
   ```
   The API will start on `http://localhost:5083`

5. **Run the Blazor Client** (Terminal 2)
   ```bash
   cd FOMSApp.Client
   dotnet run
   ```
   The client will start on `http://localhost:5187` (port may vary)

6. **Open the application**
   
   Navigate to `http://localhost:5187` in your browser.

### API Documentation

Swagger UI is available at `http://localhost:5083/swagger` when running in Development mode.

---

## 🔌 API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/vaults` | Get all vaults with photos |
| `GET` | `/api/vaults/{id}` | Get a specific vault |
| `POST` | `/api/vaults` | Create a new vault |
| `PUT` | `/api/vaults/{id}` | Update a vault |
| `DELETE` | `/api/vaults/{id}` | Delete a vault and its photos |
| `GET` | `/api/midpoints` | Get all midpoints with photos |
| `GET` | `/api/midpoints/{id}` | Get a specific midpoint |
| `POST` | `/api/midpoints` | Create a new midpoint |
| `PUT` | `/api/midpoints/{id}` | Update a midpoint |
| `DELETE` | `/api/midpoints/{id}` | Delete a midpoint and its photos |
| `GET` | `/api/cables` | Get all cables |
| `POST` | `/api/cables` | Create a new cable |
| `PUT` | `/api/cables/{id}` | Update a cable |
| `DELETE` | `/api/cables/{id}` | Delete a cable |
| `POST` | `/api/photos` | Upload a photo |
| `DELETE` | `/api/photos/{id}` | Delete a photo |

---

## ☁️ Azure Cloud Roadmap

The following Azure services are planned to transform this application into a cloud-native solution:

### Phase 1: Cloud Database & Hosting 🎯
| Service | Purpose | Status |
|---------|---------|--------|
| **Azure SQL Database** | Managed database with geo-redundancy and automatic backups | 📋 Planned |
| **Azure App Service** | Host API and Blazor client with auto-scaling | 📋 Planned |
| **Azure Static Web Apps** | Alternative: Host Blazor WASM as static site with global CDN | 📋 Planned |

### Phase 2: Cloud Storage 📦
| Service | Purpose | Status |
|---------|---------|--------|
| **Azure Blob Storage** | Scalable photo storage replacing local `wwwroot/uploads` | 📋 Planned |
| **Azure CDN** | Fast global delivery of photos and static assets | 📋 Planned |

### Phase 3: Security & Identity 🔐
| Service | Purpose | Status |
|---------|---------|--------|
| **Azure Key Vault** | Secure storage for connection strings and API keys | 📋 Planned |
| **Azure Active Directory (Entra ID)** | User authentication and role-based access control | 📋 Planned |

### Phase 4: Monitoring & DevOps 📊
| Service | Purpose | Status |
|---------|---------|--------|
| **Azure Application Insights** | Telemetry, performance monitoring, and error tracking | 📋 Planned |
| **Azure DevOps / GitHub Actions** | CI/CD pipelines for automated deployment | 📋 Planned |

### Phase 5: Advanced Features ⚡
| Service | Purpose | Status |
|---------|---------|--------|
| **Azure Functions** | Serverless photo processing (resize, compress, thumbnails) | 📋 Planned |
| **Azure SignalR Service** | Real-time map updates when multiple users are editing | 📋 Planned |
| **Azure Maps** | Enterprise mapping with enhanced satellite imagery | 📋 Planned |
| **Azure Cognitive Services** | AI-powered photo analysis (detect vault lid open/closed) | 📋 Planned |

### Future Integrations 🔮
| Integration | Purpose | Status |
|-------------|---------|--------|
| **Google Drive API** | Auto-sync approved photos to client's Google Drive | 📋 Planned |
| **.NET MAUI Mobile App** | Native mobile app for offline field crew usage | 📋 Planned |
| **Push Notifications** | Alert crews when photos are rejected | 📋 Planned |

---

## Screenshots

*Coming soon: Screenshots of the map interface, photo upload workflow, and status tracking*

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👤 Author

*Your Name*  
*Your Email or GitHub Profile*

---

## 🙏 Acknowledgments

- [Leaflet.js](https://leafletjs.com/) - Interactive maps
- [OpenStreetMap](https://www.openstreetmap.org/) - Map tile data
- [Esri](https://www.esri.com/) - Satellite imagery
- [NetTopologySuite](https://github.com/NetTopologySuite/NetTopologySuite) - Spatial data handling
- [Bootstrap](https://getbootstrap.com/) - UI framework
