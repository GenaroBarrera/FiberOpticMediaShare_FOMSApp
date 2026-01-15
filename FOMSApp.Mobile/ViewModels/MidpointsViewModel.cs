using FOMSApp.Mobile.Services;
using FOMSApp.Shared.Models;

namespace FOMSApp.Mobile.ViewModels;

public class MidpointsViewModel : BindableObject
{
    private readonly IApiService _apiService;
    private List<Midpoint> _midpoints = new();

    public List<Midpoint> Midpoints
    {
        get => _midpoints;
        set
        {
            _midpoints = value;
            OnPropertyChanged();
        }
    }

    public MidpointsViewModel(IApiService apiService)
    {
        _apiService = apiService;
        LoadMidpoints();
    }

    private async void LoadMidpoints()
    {
        try
        {
            Midpoints = await _apiService.GetMidpointsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading midpoints: {ex.Message}");
        }
    }
}
