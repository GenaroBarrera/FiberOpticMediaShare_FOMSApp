using FOMSApp.Mobile.Services;
using FOMSApp.Shared.Models;

namespace FOMSApp.Mobile.ViewModels;

public class CablesViewModel : BindableObject
{
    private readonly IApiService _apiService;
    private List<Cable> _cables = new();

    public List<Cable> Cables
    {
        get => _cables;
        set
        {
            _cables = value;
            OnPropertyChanged();
        }
    }

    public CablesViewModel(IApiService apiService)
    {
        _apiService = apiService;
        LoadCables();
    }

    private async void LoadCables()
    {
        try
        {
            Cables = await _apiService.GetCablesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading cables: {ex.Message}");
        }
    }
}
