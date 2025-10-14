namespace RVR.Framework.Plugins.Abstractions;

/// <summary>
/// Loads plugin assemblies from a directory and discovers <see cref="IPlugin"/> implementations.
/// </summary>
public interface IPluginLoader
{
    /// <summary>
    /// Scans the specified directory for plugin assemblies and loads them.
    /// </summary>
    /// <param name="directory">The directory to scan for plugin DLLs.</param>
    /// <returns>A collection of discovered plugin instances.</returns>
    IReadOnlyList<IPlugin> LoadPlugins(string directory);

    /// <summary>
    /// Returns all plugins that have been loaded so far.
    /// </summary>
    /// <returns>A read-only list of loaded plugins.</returns>
    IReadOnlyList<IPlugin> GetLoadedPlugins();
}
