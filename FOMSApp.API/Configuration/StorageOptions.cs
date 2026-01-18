namespace FOMSApp.API.Configuration;

/// <summary>
/// Configuration options for storage service selection.
/// </summary>
public class StorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>
    /// Storage provider type: "Local" or "Azure"
    /// </summary>
    public string Provider { get; set; } = "Local";

    /// <summary>
    /// Azure Blob Storage connection string (required when Provider is "Azure")
    /// </summary>
    public string? AzureConnectionString { get; set; }

    /// <summary>
    /// Azure Blob Storage container name (default: "photos")
    /// </summary>
    public string AzureContainerName { get; set; } = "photos";

    /// <summary>
    /// Optional base URL for Azure Blob Storage (useful for CDN)
    /// </summary>
    public string? AzureBaseUrl { get; set; }
}
