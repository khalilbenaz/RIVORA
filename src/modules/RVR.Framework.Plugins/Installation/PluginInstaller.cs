using System.IO.Compression;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RVR.Framework.Plugins.Abstractions;
using RVR.Framework.Plugins.Discovery;
using RVR.Framework.Plugins.Models;
using RVR.Framework.Plugins.Security;

namespace RVR.Framework.Plugins.Installation;

/// <summary>
/// Installs, uninstalls, and enumerates RIVORA plugins from NuGet packages.
/// Downloads packages via <see cref="IHttpClientFactory"/>, optionally verifies signatures,
/// checks compatibility, and extracts assemblies to the configured plugin directory.
/// </summary>
public sealed class PluginInstaller
{
    private const string NuGetPackageBaseUrl = "https://api.nuget.org/v3-flatcontainer";
    private const string InstalledManifestFileName = ".rvr-plugins.json";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PluginSignatureVerifier _signatureVerifier;
    private readonly PluginCompatibilityChecker _compatibilityChecker;
    private readonly PluginOptions _pluginOptions;
    private readonly PluginSecurityOptions _securityOptions;
    private readonly ILogger<PluginInstaller> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="PluginInstaller"/>.
    /// </summary>
    /// <param name="httpClientFactory">Factory for creating HTTP clients.</param>
    /// <param name="signatureVerifier">Verifier for NuGet package signatures.</param>
    /// <param name="compatibilityChecker">Checker for plugin-framework compatibility.</param>
    /// <param name="pluginOptions">General plugin options (e.g. plugin directory).</param>
    /// <param name="securityOptions">Security options (e.g. require signatures).</param>
    /// <param name="logger">The logger.</param>
    public PluginInstaller(
        IHttpClientFactory httpClientFactory,
        PluginSignatureVerifier signatureVerifier,
        PluginCompatibilityChecker compatibilityChecker,
        IOptions<PluginOptions> pluginOptions,
        IOptions<PluginSecurityOptions> securityOptions,
        ILogger<PluginInstaller> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _signatureVerifier = signatureVerifier ?? throw new ArgumentNullException(nameof(signatureVerifier));
        _compatibilityChecker = compatibilityChecker ?? throw new ArgumentNullException(nameof(compatibilityChecker));
        _pluginOptions = pluginOptions?.Value ?? throw new ArgumentNullException(nameof(pluginOptions));
        _securityOptions = securityOptions?.Value ?? throw new ArgumentNullException(nameof(securityOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Installs a plugin from NuGet by downloading the package, verifying its signature (if configured),
    /// and extracting assemblies to the plugin directory.
    /// </summary>
    /// <param name="packageId">The NuGet package ID to install.</param>
    /// <param name="version">An optional specific version; when <c>null</c>, the latest is resolved.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The result of the installation attempt.</returns>
    public async Task<PluginInstallResult> InstallAsync(
        string packageId,
        string? version = null,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

        _logger.LogInformation("Installing plugin '{PackageId}' (version: {Version})...", packageId, version ?? "latest");

        try
        {
            // Resolve version if not specified
            var resolvedVersion = version ?? await ResolveLatestVersionAsync(packageId, ct).ConfigureAwait(false);

            if (string.IsNullOrEmpty(resolvedVersion))
            {
                return new PluginInstallResult(false, packageId, string.Empty, "Could not resolve package version.");
            }

            // Download the .nupkg
            var nupkgPath = await DownloadPackageAsync(packageId, resolvedVersion, ct).ConfigureAwait(false);

            if (nupkgPath is null)
            {
                return new PluginInstallResult(false, packageId, resolvedVersion, "Failed to download package.");
            }

            // Verify signature if required
            if (_securityOptions.RequireSignedPackages)
            {
                var signatureResult = await _signatureVerifier.VerifyAsync(nupkgPath, ct).ConfigureAwait(false);

                if (!signatureResult.IsSigned || !signatureResult.IsTrusted)
                {
                    CleanupTempFile(nupkgPath);
                    return new PluginInstallResult(
                        false, packageId, resolvedVersion,
                        "Package signature verification failed. The package is not signed or not trusted.");
                }
            }

            // Extract to plugin directory
            var installPath = ExtractPackage(packageId, resolvedVersion, nupkgPath);
            CleanupTempFile(nupkgPath);

            // Record the installation
            await RecordInstallationAsync(packageId, resolvedVersion, installPath, ct).ConfigureAwait(false);

            _logger.LogInformation(
                "Successfully installed plugin '{PackageId}' v{Version} to '{Path}'.",
                packageId, resolvedVersion, installPath);

            return new PluginInstallResult(true, packageId, resolvedVersion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to install plugin '{PackageId}'.", packageId);
            return new PluginInstallResult(false, packageId, version ?? string.Empty, ex.Message);
        }
    }

    /// <summary>
    /// Uninstalls a plugin by removing its files from the plugin directory.
    /// </summary>
    /// <param name="packageId">The NuGet package ID to uninstall.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns><c>true</c> if the plugin was found and removed; <c>false</c> otherwise.</returns>
    public async Task<bool> UninstallAsync(string packageId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

        _logger.LogInformation("Uninstalling plugin '{PackageId}'...", packageId);

        var manifest = await LoadManifestAsync(ct).ConfigureAwait(false);
        var entry = manifest.Find(e =>
            string.Equals(e.PackageId, packageId, StringComparison.OrdinalIgnoreCase));

        if (entry is null)
        {
            _logger.LogWarning("Plugin '{PackageId}' is not installed.", packageId);
            return false;
        }

        // Remove files
        if (Directory.Exists(entry.InstallPath))
        {
            Directory.Delete(entry.InstallPath, recursive: true);
            _logger.LogInformation("Removed plugin directory '{Path}'.", entry.InstallPath);
        }

        manifest.Remove(entry);
        await SaveManifestAsync(manifest, ct).ConfigureAwait(false);

        _logger.LogInformation("Successfully uninstalled plugin '{PackageId}'.", packageId);
        return true;
    }

    /// <summary>
    /// Lists all plugins that have been installed via this installer.
    /// </summary>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A list of installed plugin information records.</returns>
    public async Task<IReadOnlyList<InstalledPluginInfo>> ListInstalledAsync(CancellationToken ct = default)
    {
        var manifest = await LoadManifestAsync(ct).ConfigureAwait(false);
        return manifest.AsReadOnly();
    }

    #region Private helpers

    private async Task<string?> ResolveLatestVersionAsync(string packageId, CancellationToken ct)
    {
        using var client = _httpClientFactory.CreateClient("PluginInstaller");
        var url = $"{NuGetPackageBaseUrl}/{packageId.ToLowerInvariant()}/index.json";

        using var response = await client.GetAsync(url, ct).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to resolve versions for '{PackageId}': HTTP {StatusCode}.", packageId, response.StatusCode);
            return null;
        }

        using var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct).ConfigureAwait(false);

        if (doc.RootElement.TryGetProperty("versions", out var versions))
        {
            var allVersions = versions.EnumerateArray().Select(v => v.GetString()).Where(v => v is not null).ToList();
            return allVersions.LastOrDefault();
        }

        return null;
    }

    private async Task<string?> DownloadPackageAsync(string packageId, string version, CancellationToken ct)
    {
        using var client = _httpClientFactory.CreateClient("PluginInstaller");
        var lowerId = packageId.ToLowerInvariant();
        var lowerVersion = version.ToLowerInvariant();
        var url = $"{NuGetPackageBaseUrl}/{lowerId}/{lowerVersion}/{lowerId}.{lowerVersion}.nupkg";

        _logger.LogDebug("Downloading package from '{Url}'.", url);

        using var response = await client.GetAsync(url, ct).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to download package: HTTP {StatusCode}.", response.StatusCode);
            return null;
        }

        var tempPath = Path.Combine(Path.GetTempPath(), $"{lowerId}.{lowerVersion}.nupkg");
        await using var fileStream = File.Create(tempPath);
        await response.Content.CopyToAsync(fileStream, ct).ConfigureAwait(false);

        return tempPath;
    }

    /// <summary>
    /// Maximum total extracted size to protect against zip bombs (100 MB).
    /// </summary>
    private const long MaxExtractedSize = 100 * 1024 * 1024;

    private string ExtractPackage(string packageId, string version, string nupkgPath)
    {
        var pluginDir = Path.GetFullPath(_pluginOptions.PluginDirectory);
        var installPath = Path.Combine(pluginDir, $"{packageId}.{version}");

        if (Directory.Exists(installPath))
        {
            Directory.Delete(installPath, recursive: true);
        }

        Directory.CreateDirectory(installPath);

        using var archive = ZipFile.OpenRead(nupkgPath);

        long totalExtracted = 0;

        foreach (var entry in archive.Entries)
        {
            // Only extract lib assemblies targeting net9.0 (or netstandard2.1 as fallback)
            if (!entry.FullName.StartsWith("lib/", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!entry.FullName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) &&
                !entry.FullName.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Protect against zip bomb
            totalExtracted += entry.Length;
            if (totalExtracted > MaxExtractedSize)
            {
                Directory.Delete(installPath, recursive: true);
                throw new InvalidOperationException(
                    $"Package '{packageId}' exceeds maximum allowed extracted size ({MaxExtractedSize / (1024 * 1024)} MB). Aborting extraction.");
            }

            var fileName = Path.GetFileName(entry.FullName);

            // Validate filename to prevent path traversal
            if (string.IsNullOrEmpty(fileName) || fileName.Contains("..") || Path.IsPathRooted(fileName))
            {
                _logger.LogWarning("Skipping suspicious entry '{EntryName}' in package '{PackageId}'", entry.FullName, packageId);
                continue;
            }

            var destinationPath = Path.Combine(installPath, fileName);

            // Verify destination is within install directory
            var fullDestination = Path.GetFullPath(destinationPath);
            if (!fullDestination.StartsWith(Path.GetFullPath(installPath), StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Path traversal attempt detected in entry '{EntryName}'", entry.FullName);
                continue;
            }

            entry.ExtractToFile(destinationPath, overwrite: true);
        }

        return installPath;
    }

    private string GetManifestPath()
    {
        var pluginDir = Path.GetFullPath(_pluginOptions.PluginDirectory);
        Directory.CreateDirectory(pluginDir);
        return Path.Combine(pluginDir, InstalledManifestFileName);
    }

    private async Task<List<InstalledPluginInfo>> LoadManifestAsync(CancellationToken ct)
    {
        var path = GetManifestPath();

        if (!File.Exists(path))
        {
            return [];
        }

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<List<InstalledPluginInfo>>(stream, cancellationToken: ct)
                   .ConfigureAwait(false) ?? [];
    }

    private async Task SaveManifestAsync(List<InstalledPluginInfo> manifest, CancellationToken ct)
    {
        var path = GetManifestPath();
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, manifest, ManifestJsonOptions, ct).ConfigureAwait(false);
    }

    private async Task RecordInstallationAsync(string packageId, string version, string installPath, CancellationToken ct)
    {
        var manifest = await LoadManifestAsync(ct).ConfigureAwait(false);

        // Remove any existing entry for this package
        manifest.RemoveAll(e => string.Equals(e.PackageId, packageId, StringComparison.OrdinalIgnoreCase));

        manifest.Add(new InstalledPluginInfo(packageId, version, installPath, DateTime.UtcNow));
        await SaveManifestAsync(manifest, ct).ConfigureAwait(false);
    }

    private static void CleanupTempFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // Best-effort cleanup; not critical.
        }
    }

    private static readonly JsonSerializerOptions ManifestJsonOptions = new()
    {
        WriteIndented = true
    };

    #endregion
}

/// <summary>
/// The result of a plugin installation attempt.
/// </summary>
/// <param name="Success">Whether the installation succeeded.</param>
/// <param name="PackageId">The NuGet package ID.</param>
/// <param name="Version">The installed version.</param>
/// <param name="Error">An optional error message if the installation failed.</param>
public sealed record PluginInstallResult(
    bool Success,
    string PackageId,
    string Version,
    string? Error = null);

/// <summary>
/// Information about a plugin that has been installed via the <see cref="PluginInstaller"/>.
/// </summary>
/// <param name="PackageId">The NuGet package ID.</param>
/// <param name="Version">The installed version.</param>
/// <param name="InstallPath">The directory where the plugin was extracted.</param>
/// <param name="InstalledAt">The UTC timestamp when the plugin was installed.</param>
public sealed record InstalledPluginInfo(
    string PackageId,
    string Version,
    string InstallPath,
    DateTime InstalledAt);
