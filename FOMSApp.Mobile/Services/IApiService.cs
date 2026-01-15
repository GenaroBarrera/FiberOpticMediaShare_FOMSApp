using FOMSApp.Shared.Models;

namespace FOMSApp.Mobile.Services;

public interface IApiService
{
    Task<List<Vault>> GetVaultsAsync();
    Task<Vault?> GetVaultAsync(int id);
    Task<Vault> CreateVaultAsync(Vault vault);
    Task UpdateVaultAsync(Vault vault);
    Task DeleteVaultAsync(int id);

    Task<List<Midpoint>> GetMidpointsAsync();
    Task<Midpoint?> GetMidpointAsync(int id);
    Task<Midpoint> CreateMidpointAsync(Midpoint midpoint);
    Task UpdateMidpointAsync(Midpoint midpoint);
    Task DeleteMidpointAsync(int id);

    Task<List<Cable>> GetCablesAsync();
    Task<Cable?> GetCableAsync(int id);
    Task<Cable> CreateCableAsync(Cable cable);
    Task UpdateCableAsync(Cable cable);
    Task DeleteCableAsync(int id);

    Task<List<Photo>> GetPhotosForVaultAsync(int vaultId);
    Task<List<Photo>> GetPhotosForMidpointAsync(int midpointId);
    Task<Photo> UploadPhotoAsync(Stream photoStream, string fileName, int? vaultId, int? midpointId);
    Task DeletePhotoAsync(int photoId);
}
