# FOMSApp (Fiber Optic Media Share App)

FOMSApp is a web-based construction management tool designed to optimize the Quality Assurance (QA) workflow for fiber optic network installations.

It solves the inefficiency of manual photo management by combining **Geospatial Visualization** with **Cloud Storage Automation**. Field crews upload photos directly to map-based infrastructure points, and project coordinators review them in real-time. Upon approval ("Green" status), data is automatically synced to the client's required external storage (Google Drive).

## 🚀 Key Features

* **Interactive Construction Map:** Visualizes fiber routes (conduit) and infrastructure nodes (vaults) using Leaflet.js.
* **Geospatial Data Management:** Uses **SQL Server Spatial (NetTopologySuite)** to store exact GPS coordinates of construction assets.
* **Real-time Status Tracking:** Vault pins change color dynamically (Gray → Yellow → Green/Red) based on QA status.
* **High-Speed Uploads:** Decouples viewing from storage by uploading field photos to **Azure Blob Storage** for instant gallery loading.
* **Automated Client Delivery:** Background services automatically sync approved photos to the Client's Google Drive, eliminating manual file transfer.

## 🛠️ Tech Stack & Architecture

This project is built using **Clean Architecture** principles within the .NET ecosystem.

| Component | Technology | Purpose |
| :--- | :--- | :--- |
| **Backend** | ASP.NET Core 8 Web API | RESTful API, Business Logic, Google Drive Integration |
| **Frontend** | Blazor WebAssembly (WASM) | Client-side C# Logic, Interactive UI, Offline capabilities |
| **Database** | SQL Server + NetTopologySuite | Relational data + Spatial queries (`Geography` types) |
| **Mapping** | Leaflet.js | Rendering interactive maps and custom engineering overlays |
| **Storage** | Azure Blob Storage | High-performance "Hot" tier storage for image galleries |
| **Background Jobs**| Hangfire / BackgroundService | Handling long-running sync tasks to Google Drive |

## 🏗️ System Architecture

1.  **Ingestion:** Engineering drawings (KML/GeoJSON) are parsed to populate the `Vaults` table.
2.  **Field Ops:** Crews select a Vault on the map and upload photos via the Blazor Client -> Azure Blob.
3.  **Review:** Coordinator accepts a vault.
4.  **Sync:** The Backend triggers a job to stream the file from Azure -> Google Drive API.

## 💻 Getting Started

### Prerequisites
* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB works too)
* Azure Storage Account (or Azurite Emulator for local dev)
* Google Cloud Service Account (for Drive API testing)

### Installation

1.  **Clone the repository**
    ```bash
    git clone [https://github.com/YourUsername/FOMSApp.git](https://github.com/YourUsername/FOMSApp.git)
    cd FOMSApp
    ```

2.  **Configure Database**
    Update `appsettings.json` in `FOMSApp.API` with your connection string.
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=localhost;Database=FOMSDb;Trusted_Connection=True;TrustServerCertificate=True;"
    }
    ```

3.  **Run Migrations (Spatial Data Setup)**
    ```bash
    cd FOMSApp.API
    dotnet ef database update
    ```

4.  **Run the API**
    ```bash
    dotnet run
    ```

## 📸 Screenshots

*(Placeholder: Add screenshots here of the Map View, Photo Upload Modal, and QA Dashboard)*

## 🔮 Future Improvements (Roadmap)
* **Mobile App:** Native MAUI wrapper for better offline support in remote areas.
* **AI Analysis:** Computer Vision to auto-detect if a vault lid is open or closed.
* **Push Notifications:** Alert field crews immediately when a photo is rejected.

## 📄 License
This project is licensed under the MIT License.
