using System.Reflection;
using Microsoft.Extensions.Logging;
using RVR.Framework.Plugins.Abstractions;

namespace RVR.Framework.Plugins.Discovery;

/// <summary>
/// Checks whether an <see cref="IRvrPlugin"/> is compatible with the current RIVORA Framework version
/// and validates its dependency graph for conflicts.
/// </summary>
public sealed class PluginCompatibilityChecker
{
    /// <summary>
    /// The current RIVORA Framework version, derived from the core assembly.
    /// </summary>
    public static readonly Version CurrentFrameworkVersion =
        Assembly.GetExecutingAssembly().GetName().Version ?? new Version(3, 1, 0);

    private readonly ILogger<PluginCompatibilityChecker> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="PluginCompatibilityChecker"/>.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public PluginCompatibilityChecker(ILogger<PluginCompatibilityChecker> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Checks compatibility of the specified plugin against the current RIVORA Framework version
    /// and validates its declared dependencies.
    /// </summary>
    /// <param name="plugin">The plugin to check.</param>
    /// <returns>A result indicating whether the plugin is compatible, with any warnings or errors.</returns>
    public PluginCompatibilityResult Check(IRvrPlugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);

        var warnings = new List<string>();
        var errors = new List<string>();

        // Check minimum RIVORA version requirement
        CheckMinimumVersion(plugin, errors);

        // Check dependency availability
        CheckDependencies(plugin, warnings, errors);

        // Validate metadata
        CheckMetadata(plugin, warnings);

        var isCompatible = errors.Count == 0;

        if (isCompatible)
        {
            _logger.LogInformation(
                "Plugin '{Name}' v{Version} is compatible with RIVORA Framework v{FrameworkVersion}.",
                plugin.Name, plugin.Version, CurrentFrameworkVersion);
        }
        else
        {
            _logger.LogWarning(
                "Plugin '{Name}' v{Version} is NOT compatible: {ErrorCount} error(s).",
                plugin.Name, plugin.Version, errors.Count);
        }

        return new PluginCompatibilityResult(isCompatible, warnings, errors);
    }

    /// <summary>
    /// Checks compatibility of a plugin against a specific framework version (for testing or preview scenarios).
    /// </summary>
    /// <param name="plugin">The plugin to check.</param>
    /// <param name="frameworkVersion">The framework version to check against.</param>
    /// <returns>A result indicating whether the plugin is compatible.</returns>
    public PluginCompatibilityResult Check(IRvrPlugin plugin, Version frameworkVersion)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        ArgumentNullException.ThrowIfNull(frameworkVersion);

        var warnings = new List<string>();
        var errors = new List<string>();

        if (!Version.TryParse(plugin.MinimumRivoraVersion, out var minVersion))
        {
            errors.Add($"Plugin '{plugin.Name}' has an invalid MinimumRivoraVersion: '{plugin.MinimumRivoraVersion}'.");
        }
        else if (frameworkVersion < minVersion)
        {
            errors.Add(
                $"Plugin '{plugin.Name}' requires RIVORA Framework v{minVersion} or later, " +
                $"but the target framework version is v{frameworkVersion}.");
        }

        CheckDependencies(plugin, warnings, errors);
        CheckMetadata(plugin, warnings);

        return new PluginCompatibilityResult(errors.Count == 0, warnings, errors);
    }

    private void CheckMinimumVersion(IRvrPlugin plugin, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(plugin.MinimumRivoraVersion))
        {
            errors.Add($"Plugin '{plugin.Name}' does not specify a MinimumRivoraVersion.");
            return;
        }

        if (!Version.TryParse(plugin.MinimumRivoraVersion, out var minVersion))
        {
            errors.Add(
                $"Plugin '{plugin.Name}' has an invalid MinimumRivoraVersion format: '{plugin.MinimumRivoraVersion}'. " +
                "Expected a valid SemVer string (e.g. '3.1.0').");
            return;
        }

        if (CurrentFrameworkVersion < minVersion)
        {
            errors.Add(
                $"Plugin '{plugin.Name}' requires RIVORA Framework v{minVersion} or later, " +
                $"but the current framework version is v{CurrentFrameworkVersion}.");
        }
    }

    private static void CheckDependencies(IRvrPlugin plugin, List<string> warnings, List<string> errors)
    {
        var dependencies = plugin.Dependencies?.ToList() ?? [];

        if (dependencies.Count == 0)
        {
            return;
        }

        // Detect self-dependency
        if (dependencies.Any(d => string.Equals(d, plugin.Name, StringComparison.OrdinalIgnoreCase)))
        {
            errors.Add($"Plugin '{plugin.Name}' declares itself as a dependency, which is not allowed.");
        }

        // Detect duplicate dependencies
        var duplicates = dependencies
            .GroupBy(d => d, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        foreach (var dup in duplicates)
        {
            warnings.Add($"Plugin '{plugin.Name}' declares duplicate dependency '{dup}'.");
        }
    }

    private static void CheckMetadata(IRvrPlugin plugin, List<string> warnings)
    {
        if (plugin.Metadata is null)
        {
            warnings.Add($"Plugin '{plugin.Name}' does not provide metadata.");
            return;
        }

        if (string.IsNullOrWhiteSpace(plugin.Metadata.Author))
        {
            warnings.Add($"Plugin '{plugin.Name}' metadata is missing an Author.");
        }

        if (string.IsNullOrWhiteSpace(plugin.Metadata.Description))
        {
            warnings.Add($"Plugin '{plugin.Name}' metadata is missing a Description.");
        }
    }
}

/// <summary>
/// The result of a plugin compatibility check.
/// </summary>
/// <param name="IsCompatible">Whether the plugin is compatible with the framework.</param>
/// <param name="Warnings">Non-blocking warnings about the plugin.</param>
/// <param name="Errors">Blocking errors that prevent the plugin from being loaded.</param>
public sealed record PluginCompatibilityResult(
    bool IsCompatible,
    List<string> Warnings,
    List<string> Errors);
