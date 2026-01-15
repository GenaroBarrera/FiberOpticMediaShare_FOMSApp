using FOMSApp.Mobile.Services;
using FOMSApp.Shared.Models;

namespace FOMSApp.Mobile.ViewModels;

public class VaultsViewModel : BindableObject
{
    private readonly IApiService _apiService;
    private List<Vault> _vaults = new();

    public List<Vault> Vaults
    {
        get => _vaults;
        set
        {
            _vaults = value;
            OnPropertyChanged();
        }
    }

    public VaultsViewModel(IApiService apiService)
    {
        _apiService = apiService;
        LoadVaults();
    }

    private async void LoadVaults()
    {
        try
        {
            Vaults = await _apiService.GetVaultsAsync();
        }
        catch (Exception ex)
        {
            // Handle error
            System.Diagnostics.Debug.WriteLine($"Error loading vaults: {ex.Message}");
        }
    }
}
