using FOMSApp.Mobile.Services;
using FOMSApp.Shared.Models;

namespace FOMSApp.Mobile.ViewModels;

public class VaultDetailsViewModel : BindableObject
{
    private readonly IApiService _apiService;
    private Vault? _vault;
    private List<Photo> _photos = new();

    public Vault? Vault
    {
        get => _vault;
        set
        {
            _vault = value;
            OnPropertyChanged();
        }
    }

    public List<Photo> Photos
    {
        get => _photos;
        set
        {
            _photos = value;
            OnPropertyChanged();
        }
    }

    public VaultDetailsViewModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task LoadVaultAsync(int id)
    {
        try
        {
            Vault = await _apiService.GetVaultAsync(id);
            if (Vault != null)
            {
                Photos = await _apiService.GetPhotosForVaultAsync(id);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading vault: {ex.Message}");
        }
    }
}
