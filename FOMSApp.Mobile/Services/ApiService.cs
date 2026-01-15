using System.Net.Http.Json;
using System.Text.Json;
using FOMSApp.Shared.Models;
using NetTopologySuite.IO.Converters;

namespace FOMSApp.Mobile.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    // TODO: Update this to your Azure API URL
    private const string BaseUrl = "http://localhost:5083"; // Change to your Azure App Service URL

    public ApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl)
        };

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        _jsonOptions.Converters.Add(new GeoJsonConverterFactory());
    }

    // Vault operations
    public async Task<List<Vault>> GetVaultsAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<Vault>>("api/vaults", _jsonOptions);
        return response ?? new List<Vault>();
    }

    public async Task<Vault?> GetVaultAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<Vault>($"api/vaults/{id}", _jsonOptions);
    }

    public async Task<Vault> CreateVaultAsync(Vault vault)
    {
        var response = await _httpClient.PostAsJsonAsync("api/vaults", vault, _jsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Vault>(_jsonOptions) ?? vault;
    }

    public async Task UpdateVaultAsync(Vault vault)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/vaults/{vault.Id}", vault, _jsonOptions);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteVaultAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/vaults/{id}");
        response.EnsureSuccessStatusCode();
    }

    // Midpoint operations
    public async Task<List<Midpoint>> GetMidpointsAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<Midpoint>>("api/midpoints", _jsonOptions);
        return response ?? new List<Midpoint>();
    }

    public async Task<Midpoint?> GetMidpointAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<Midpoint>($"api/midpoints/{id}", _jsonOptions);
    }

    public async Task<Midpoint> CreateMidpointAsync(Midpoint midpoint)
    {
        var response = await _httpClient.PostAsJsonAsync("api/midpoints", midpoint, _jsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Midpoint>(_jsonOptions) ?? midpoint;
    }

    public async Task UpdateMidpointAsync(Midpoint midpoint)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/midpoints/{midpoint.Id}", midpoint, _jsonOptions);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteMidpointAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/midpoints/{id}");
        response.EnsureSuccessStatusCode();
    }

    // Cable operations
    public async Task<List<Cable>> GetCablesAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<Cable>>("api/cables", _jsonOptions);
        return response ?? new List<Cable>();
    }

    public async Task<Cable?> GetCableAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<Cable>($"api/cables/{id}", _jsonOptions);
    }

    public async Task<Cable> CreateCableAsync(Cable cable)
    {
        var response = await _httpClient.PostAsJsonAsync("api/cables", cable, _jsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Cable>(_jsonOptions) ?? cable;
    }

    public async Task UpdateCableAsync(Cable cable)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/cables/{cable.Id}", cable, _jsonOptions);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteCableAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/cables/{id}");
        response.EnsureSuccessStatusCode();
    }

    // Photo operations
    public async Task<List<Photo>> GetPhotosForVaultAsync(int vaultId)
    {
        var response = await _httpClient.GetFromJsonAsync<List<Photo>>($"api/photos/vault/{vaultId}", _jsonOptions);
        return response ?? new List<Photo>();
    }

    public async Task<List<Photo>> GetPhotosForMidpointAsync(int midpointId)
    {
        var response = await _httpClient.GetFromJsonAsync<List<Photo>>($"api/photos/midpoint/{midpointId}", _jsonOptions);
        return response ?? new List<Photo>();
    }

    public async Task<Photo> UploadPhotoAsync(Stream photoStream, string fileName, int? vaultId, int? midpointId)
    {
        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(photoStream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(streamContent, "file", fileName);

        if (vaultId.HasValue)
            content.Add(new StringContent(vaultId.Value.ToString()), "vaultId");
        if (midpointId.HasValue)
            content.Add(new StringContent(midpointId.Value.ToString()), "midpointId");

        var response = await _httpClient.PostAsync("api/photos", content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Photo>(_jsonOptions) ?? new Photo();
    }

    public async Task DeletePhotoAsync(int photoId)
    {
        var response = await _httpClient.DeleteAsync($"api/photos/{photoId}");
        response.EnsureSuccessStatusCode();
    }
}
