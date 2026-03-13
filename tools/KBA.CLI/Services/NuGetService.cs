using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Spectre.Console;

namespace KBA.CLI.Services;

/// <summary>
/// Provides services for interacting with NuGet.org API to check for package updates.
/// </summary>
public class NuGetService
{
    private readonly HttpClient _httpClient;
    private readonly IAnsiConsole _console;

    /// <summary>
    /// Initializes a new instance of the <see cref="NuGetService"/> class.
    /// </summary>
    /// <param name="console">The ANSI console for output.</param>
    public NuGetService(IAnsiConsole console)
    {
        _console = console;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.nuget.org/"),
            Timeout = TimeSpan.FromSeconds(30)
        };
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "KBA.CLI/2.0");
    }

    /// <summary>
    /// Gets the latest version of a package from NuGet.org.
    /// </summary>
    /// <param name="packageName">The name of the package.</param>
    /// <param name="includePreview">Whether to include preview versions.</param>
    /// <param name="includeNightly">Whether to include nightly builds.</param>
    /// <returns>The latest version information, or null if not found.</returns>
    public async Task<PackageVersionInfo?> GetLatestVersionAsync(
        string packageName,
        bool includePreview = false,
        bool includeNightly = false)
    {
        try
        {
            // First, get the package registration index
            var registrationUrl = $"v3/registration5-gz-semver2/{packageName.ToLower()}/index.json";
            var registrationResponse = await _httpClient.GetAsync(registrationUrl);

            if (!registrationResponse.IsSuccessStatusCode)
            {
                return null;
            }

            var registrationData = await registrationResponse.Content.ReadFromJsonAsync<RegistrationIndex>();
            if (registrationData?.Items == null || registrationData.Items.Count == 0)
            {
                return null;
            }

            // Get all versions from all pages
            var allVersions = new List<string>();
            foreach (var item in registrationData.Items)
            {
                if (item.Items != null && item.Items.Count > 0)
                {
                    // Items are already in the response
                    allVersions.AddRange(item.Items.Select(v => v.Version));
                }
                else if (!string.IsNullOrEmpty(item.Url))
                {
                    // Need to fetch the page
                    var pageResponse = await _httpClient.GetAsync(item.Url);
                    if (pageResponse.IsSuccessStatusCode)
                    {
                        var pageData = await pageResponse.Content.ReadFromJsonAsync<RegistrationPage>();
                        if (pageData?.Items != null)
                        {
                            allVersions.AddRange(pageData.Items.Select(v => v.Version));
                        }
                    }
                }
            }

            if (allVersions.Count == 0)
            {
                return null;
            }

            // Filter versions based on options
            var filteredVersions = FilterVersions(allVersions, includePreview, includeNightly);

            if (filteredVersions.Count == 0)
            {
                return null;
            }

            // Get the latest version
            var latestVersion = filteredVersions.MaxBy(v => SemanticVersion.Parse(v));

            // Get package details
            var packageUrl = $"v3-flatcontainer/{packageName.ToLower()}/{latestVersion}/{packageName.ToLower()}.nuspec";
            var packageResponse = await _httpClient.GetAsync(packageUrl);

            string? description = null;
            string? projectUrl = null;
            if (packageResponse.IsSuccessStatusCode)
            {
                var nuspecContent = await packageResponse.Content.ReadAsStringAsync();
                description = ExtractNuspecValue(nuspecContent, "description");
                projectUrl = ExtractNuspecValue(nuspecContent, "projectUrl");
            }

            return new PackageVersionInfo
            {
                PackageName = packageName,
                CurrentVersion = null, // Will be set by caller
                LatestVersion = latestVersion,
                IsPreview = IsPreviewVersion(latestVersion),
                IsNightly = IsNightlyVersion(latestVersion),
                Description = description,
                ProjectUrl = projectUrl,
                DownloadUrl = $"https://www.nuget.org/packages/{packageName}/{latestVersion}"
            };
        }
        catch (HttpRequestException ex)
        {
            _console.MarkupLine($"[yellow]Warning: Failed to fetch package info for {packageName}: {ex.Message}[/]");
            return null;
        }
        catch (JsonException ex)
        {
            _console.MarkupLine($"[yellow]Warning: Failed to parse package info for {packageName}: {ex.Message}[/]");
            return null;
        }
        catch (Exception ex)
        {
            _console.MarkupLine($"[yellow]Warning: Error checking package {packageName}: {ex.Message}[/]");
            return null;
        }
    }

    /// <summary>
    /// Filters versions based on channel options.
    /// </summary>
    private static List<string> FilterVersions(IEnumerable<string> versions, bool includePreview, bool includeNightly)
    {
        var filtered = versions.ToList();

        if (!includeNightly)
        {
            filtered = filtered.Where(v => !IsNightlyVersion(v)).ToList();
        }

        if (!includePreview)
        {
            filtered = filtered.Where(v => IsStableVersion(v)).ToList();
        }

        return filtered;
    }

    /// <summary>
    /// Determines if a version is a nightly build.
    /// </summary>
    private static bool IsNightlyVersion(string version)
    {
        return version.Contains("nightly", StringComparison.OrdinalIgnoreCase) ||
               version.Contains("ci", StringComparison.OrdinalIgnoreCase) ||
               version.Contains("dev", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines if a version is a preview/release candidate.
    /// </summary>
    private static bool IsPreviewVersion(string version)
    {
        return !IsStableVersion(version) && !IsNightlyVersion(version);
    }

    /// <summary>
    /// Determines if a version is stable.
    /// </summary>
    private static bool IsStableVersion(string version)
    {
        // Stable versions don't have prerelease labels
        var parts = version.Split('-');
        return parts.Length == 1;
    }

    /// <summary>
    /// Extracts a value from a .nuspec XML content.
    /// </summary>
    private static string? ExtractNuspecValue(string nuspecContent, string elementName)
    {
        var pattern = $"<{elementName}>([^<]*)</{elementName}>";
        var match = System.Text.RegularExpressions.Regex.Match(nuspecContent, pattern);
        return match.Success ? match.Groups[1].Value : null;
    }
}

/// <summary>
/// Represents information about a package version.
/// </summary>
public class PackageVersionInfo
{
    /// <summary>
    /// Gets or sets the package name.
    /// </summary>
    public string PackageName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current installed version.
    /// </summary>
    public string? CurrentVersion { get; set; }

    /// <summary>
    /// Gets or sets the latest available version.
    /// </summary>
    public string LatestVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the latest version is a preview.
    /// </summary>
    public bool IsPreview { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the latest version is a nightly build.
    /// </summary>
    public bool IsNightly { get; set; }

    /// <summary>
    /// Gets or sets the package description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the project URL.
    /// </summary>
    public string? ProjectUrl { get; set; }

    /// <summary>
    /// Gets or sets the NuGet download URL.
    /// </summary>
    public string DownloadUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether an update is available.
    /// </summary>
    public bool HasUpdate => CurrentVersion != null &&
                             !string.Equals(CurrentVersion, LatestVersion, StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// Represents a semantic version for comparison.
/// </summary>
public class SemanticVersion : IComparable<SemanticVersion>
{
    public Version Version { get; set; } = new Version(0, 0, 0, 0);
    public string PrereleaseLabel { get; set; } = string.Empty;
    public int PrereleaseNumber { get; set; } = int.MaxValue;

    public static SemanticVersion Parse(string version)
    {
        var result = new SemanticVersion();

        // Split prerelease label
        var parts = version.Split('-', 2);
        var versionPart = parts[0];
        var prereleasePart = parts.Length > 1 ? parts[1] : null;

        // Parse main version
        var versionNumbers = versionPart.Split('.');
        var major = versionNumbers.Length > 0 && int.TryParse(versionNumbers[0], out var m) ? m : 0;
        var minor = versionNumbers.Length > 1 && int.TryParse(versionNumbers[1], out var mi) ? mi : 0;
        var patch = versionNumbers.Length > 2 && int.TryParse(versionNumbers[2], out var p) ? p : 0;
        var build = versionNumbers.Length > 3 && int.TryParse(versionNumbers[3], out var b) ? b : 0;

        result.Version = new Version(major, minor, patch, build);

        // Parse prerelease
        if (!string.IsNullOrEmpty(prereleasePart))
        {
            result.PrereleaseLabel = prereleasePart;

            // Extract number from prerelease (e.g., "alpha.1" -> 1)
            var labelParts = prereleasePart.Split('.');
            if (labelParts.Length > 1 && int.TryParse(labelParts[^1], out var num))
            {
                result.PrereleaseNumber = num;
            }
        }

        return result;
    }

    public int CompareTo(SemanticVersion? other)
    {
        if (other == null) return 1;

        var versionComparison = Version.CompareTo(other.Version);
        if (versionComparison != 0) return versionComparison;

        // If one has prerelease and the other doesn't, stable wins
        if (string.IsNullOrEmpty(PrereleaseLabel) && !string.IsNullOrEmpty(other.PrereleaseLabel)) return 1;
        if (!string.IsNullOrEmpty(PrereleaseLabel) && string.IsNullOrEmpty(other.PrereleaseLabel)) return -1;

        // Both have prerelease, compare labels
        if (!string.IsNullOrEmpty(PrereleaseLabel) && !string.IsNullOrEmpty(other.PrereleaseLabel))
        {
            var labelCompare = ComparePrereleaseLabels(PrereleaseLabel, other.PrereleaseLabel);
            if (labelCompare != 0) return labelCompare;

            // Same label, compare numbers
            return PrereleaseNumber.CompareTo(other.PrereleaseNumber);
        }

        return 0;
    }

    private static int ComparePrereleaseLabels(string a, string b)
    {
        // Order: alpha < beta < rc < preview < nightly
        var order = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "alpha", 1 },
            { "beta", 2 },
            { "rc", 3 },
            { "preview", 4 },
            { "nightly", 5 },
            { "ci", 6 },
            { "dev", 7 }
        };

        var aBase = a.Split('.').FirstOrDefault() ?? a;
        var bBase = b.Split('.').FirstOrDefault() ?? b;

        if (order.TryGetValue(aBase, out var aOrder) && order.TryGetValue(bBase, out var bOrder))
        {
            return aOrder.CompareTo(bOrder);
        }

        return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
    }

    public override string ToString() => Version.ToString();
}

/// <summary>
/// NuGet registration index response model.
/// </summary>
public class RegistrationIndex
{
    [JsonPropertyName("items")]
    public List<RegistrationPage> Items { get; set; } = new();
}

/// <summary>
/// NuGet registration page response model.
/// </summary>
public class RegistrationPage
{
    [JsonPropertyName("@id")]
    public string? Url { get; set; }

    [JsonPropertyName("items")]
    public List<PackageVersionItem> Items { get; set; } = new();
}

/// <summary>
/// NuGet package version item model.
/// </summary>
public class PackageVersionItem
{
    [JsonPropertyName("catalogEntry")]
    public CatalogEntry? CatalogEntry { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// NuGet catalog entry model.
/// </summary>
public class CatalogEntry
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;
}
