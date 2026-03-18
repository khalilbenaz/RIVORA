using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace RVR.Framework.Plugins.Abstractions;

/// <summary>
/// Enhanced plugin interface with RIVORA framework awareness.
/// Extends <see cref="IPlugin"/> with version compatibility, dependency tracking, and rich metadata.
/// </summary>
public interface IRvrPlugin : IPlugin
{
    /// <summary>
    /// The minimum RIVORA Framework version required by this plugin (SemVer format, e.g. "3.1.0").
    /// </summary>
    string MinimumRivoraVersion { get; }

    /// <summary>
    /// A list of plugin names (package IDs) that this plugin depends on.
    /// Dependencies are resolved and loaded before this plugin is initialized.
    /// </summary>
    IEnumerable<string> Dependencies { get; }

    /// <summary>
    /// Rich metadata describing the plugin, including author, description, and licensing.
    /// </summary>
    PluginMetadata Metadata { get; }
}

/// <summary>
/// Describes metadata for a RIVORA plugin including authorship, licensing, and discoverability tags.
/// </summary>
/// <param name="Author">The plugin author or organization.</param>
/// <param name="Description">A short description of what the plugin does.</param>
/// <param name="ProjectUrl">An optional URL to the plugin's source repository or homepage.</param>
/// <param name="LicenseExpression">An optional SPDX license expression (e.g. "MIT", "Apache-2.0").</param>
/// <param name="Tags">An optional set of tags for search and categorization.</param>
public sealed record PluginMetadata(
    string Author,
    string Description,
    string? ProjectUrl = null,
    string? LicenseExpression = null,
    IReadOnlyList<string>? Tags = null);
