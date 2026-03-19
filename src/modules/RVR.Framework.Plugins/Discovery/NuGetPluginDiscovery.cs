using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace RVR.Framework.Plugins.Discovery;

/// <summary>
/// Discovers RIVORA plugins from the NuGet.org v3 API by searching for packages
/// that follow the <c>RVR.Plugin.*</c> or <c>RVR.Module.*</c> naming convention.
/// </summary>
public sealed class NuGetPluginDiscovery
{
    private const string DefaultSearchUrl = "https://azuresearch-usnc.nuget.org/query";
    private const string DefaultRegistrationUrl = "https://api.nuget.org/v3/registration5-gz-semver2";
    private static readonly string[] PluginPrefixes = ["RVR.Plugin.", "RVR.Module."];

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<NuGetPluginDiscovery> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="NuGetPluginDiscovery"/>.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory for outgoing NuGet API calls.</param>
    /// <param name="logger">The logger.</param>
    public NuGetPluginDiscovery(
        IHttpClientFactory httpClientFactory,
        ILogger<NuGetPluginDiscovery> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Searches the NuGet gallery for packages matching the RIVORA plugin naming convention.
    /// </summary>
    /// <param name="query">An optional search term to further filter results.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A list of discovered plugin packages.</returns>
    public async Task<IReadOnlyList<PluginPackageInfo>> SearchPluginsAsync(
        string? query = null,
        CancellationToken ct = default)
    {
        var results = new List<PluginPackageInfo>();

        foreach (var prefix in PluginPrefixes)
        {
            var searchTerm = string.IsNullOrWhiteSpace(query) ? prefix : $"{prefix}{query}";

            try
            {
                var packages = await SearchNuGetAsync(searchTerm, ct).ConfigureAwait(false);
                results.AddRange(packages);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to search NuGet for prefix '{Prefix}'.", prefix);
            }
        }

        return results.AsReadOnly();
    }

    /// <summary>
    /// Retrieves detailed information about a specific NuGet package by its package ID.
    /// </summary>
    /// <param name="packageId">The NuGet package ID (e.g. "RVR.Plugin.MyPlugin").</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The package info if found; otherwise <c>null</c>.</returns>
    public async Task<PluginPackageInfo?> GetPluginInfoAsync(
        string packageId,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

        try
        {
            var results = await SearchNuGetAsync(packageId, ct).ConfigureAwait(false);
            return results.FirstOrDefault(p =>
                string.Equals(p.PackageId, packageId, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve NuGet package info for '{PackageId}'.", packageId);
            return null;
        }
    }

    private async Task<List<PluginPackageInfo>> SearchNuGetAsync(
        string searchTerm,
        CancellationToken ct)
    {
        using var client = _httpClientFactory.CreateClient("NuGetPluginDiscovery");
        var requestUri = $"{DefaultSearchUrl}?q={Uri.EscapeDataString(searchTerm)}&take=50&prerelease=false&semVerLevel=2.0.0";

        _logger.LogDebug("Querying NuGet search API: {Uri}", requestUri);

        using var response = await client.GetAsync(requestUri, ct).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var searchResponse = await response.Content
            .ReadFromJsonAsync<NuGetSearchResponse>(JsonOptions, ct)
            .ConfigureAwait(false);

        if (searchResponse?.Data is null)
        {
            return [];
        }

        var packages = new List<PluginPackageInfo>();

        foreach (var item in searchResponse.Data)
        {
            var isRvrPlugin = PluginPrefixes.Any(prefix =>
                item.Id.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

            packages.Add(new PluginPackageInfo(
                PackageId: item.Id,
                Version: item.Version,
                Description: item.Description ?? string.Empty,
                Authors: string.Join(", ", item.Authors ?? []),
                DownloadCount: item.TotalDownloads,
                IsRvrPlugin: isRvrPlugin));
        }

        return packages;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    #region NuGet API response models

    private sealed class NuGetSearchResponse
    {
        [JsonPropertyName("data")]
        public List<NuGetSearchResult>? Data { get; set; }
    }

    private sealed class NuGetSearchResult
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("authors")]
        public List<string>? Authors { get; set; }

        [JsonPropertyName("totalDownloads")]
        public long TotalDownloads { get; set; }
    }

    #endregion
}

/// <summary>
/// Represents information about a NuGet package that may be a RIVORA plugin.
/// </summary>
/// <param name="PackageId">The NuGet package identifier.</param>
/// <param name="Version">The latest stable version of the package.</param>
/// <param name="Description">A description of the package.</param>
/// <param name="Authors">A comma-separated list of package authors.</param>
/// <param name="DownloadCount">The total number of downloads.</param>
/// <param name="IsRvrPlugin">Whether the package follows the RIVORA plugin naming convention.</param>
public sealed record PluginPackageInfo(
    string PackageId,
    string Version,
    string Description,
    string Authors,
    long DownloadCount,
    bool IsRvrPlugin);
