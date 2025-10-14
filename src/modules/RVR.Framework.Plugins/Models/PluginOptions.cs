namespace RVR.Framework.Plugins.Models;

/// <summary>
/// Configuration options for the RIVORA Framework Plugin system.
/// </summary>
public class PluginOptions
{
    /// <summary>
    /// The directory to scan for plugin assemblies. Defaults to "plugins/".
    /// </summary>
    public string PluginDirectory { get; set; } = "plugins/";

    /// <summary>
    /// Whether to automatically load plugins on application startup. Defaults to true.
    /// </summary>
    public bool AutoLoadOnStartup { get; set; } = true;
}
