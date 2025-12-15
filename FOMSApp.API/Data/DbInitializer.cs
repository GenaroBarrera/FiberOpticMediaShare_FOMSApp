using NetTopologySuite;
using NetTopologySuite.Geometries;
using FOMSApp.API.Models;

namespace FOMSApp.API.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            // 1. Check if there are already vaults (Don't run if we have data)
            if (context.Vaults.Any())
            {
                return;
            }

            // 2. Setup the Geometry Factory (Required to create Points)
            // SRID 4326 is the standard for GPS (WGS 84)
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

            // 3. Create Dummy Data
            var vaults = new Vault[]
            {
                new Vault
                {
                    Name = "V-101 (Downtown)",
                    // Note: Coordinates are Longitude (X), Latitude (Y)
                    Location = geometryFactory.CreatePoint(new Coordinate(-97.7431, 30.2672)), 
                    Status = VaultStatus.Pending
                },
                new Vault
                {
                    Name = "V-102 (North Route)",
                    Location = geometryFactory.CreatePoint(new Coordinate(-97.7445, 30.2685)),
                    Status = VaultStatus.Review // Yellow
                },
                 new Vault
                {
                    Name = "V-103 (Completed)",
                    Location = geometryFactory.CreatePoint(new Coordinate(-97.7410, 30.2660)),
                    Status = VaultStatus.Complete // Green
                }
            };

            // 4. Save to DB
            context.Vaults.AddRange(vaults);
            context.SaveChanges();
        }
    }
}