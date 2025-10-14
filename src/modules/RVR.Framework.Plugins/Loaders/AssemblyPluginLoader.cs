using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;
using RVR.Framework.Plugins.Abstractions;

namespace RVR.Framework.Plugins.Loaders;

/// <summary>
/// Loads plugin assemblies from a directory, scanning for types that implement <see cref="IPlugin"/>.
/// </summary>
public class AssemblyPluginLoader : IPluginLoader
{
    private readonly ILogger<AssemblyPluginLoader> _logger;
    private readonly List<IPlugin> _loadedPlugins = [];

    /// <summary>
    /// Initializes a new instance of <see cref="AssemblyPluginLoader"/>.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public AssemblyPluginLoader(ILogger<AssemblyPluginLoader> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public IReadOnlyList<IPlugin> LoadPlugins(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);

        if (!Directory.Exists(directory))
        {
            _logger.LogWarning("Plugin directory '{Directory}' does not exist. No plugins loaded.", directory);
            return [];
        }

        var dllFiles = Directory.GetFiles(directory, "*.dll", SearchOption.TopDirectoryOnly);
        _logger.LogInformation("Scanning {Count} DLL files in '{Directory}' for plugins.", dllFiles.Length, directory);

        foreach (var dllFile in dllFiles)
        {
            try
            {
                var loadContext = new AssemblyLoadContext(
                    Path.GetFileNameWithoutExtension(dllFile), isCollectible: true);
                var assembly = loadContext.LoadFromAssemblyPath(Path.GetFullPath(dllFile));

                var pluginTypes = assembly.GetTypes()
                    .Where(t => typeof(IPlugin).IsAssignableFrom(t) && t is { IsAbstract: false, IsInterface: false });

                foreach (var pluginType in pluginTypes)
                {
                    if (Activator.CreateInstance(pluginType) is IPlugin plugin)
                    {
                        _loadedPlugins.Add(plugin);
                        _logger.LogInformation(
                            "Loaded plugin '{Name}' v{Version} from '{File}'.",
                            plugin.Name, plugin.Version, Path.GetFileName(dllFile));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load plugin from '{File}'.", Path.GetFileName(dllFile));
            }
        }

        return _loadedPlugins.AsReadOnly();
    }

    /// <inheritdoc />
    public IReadOnlyList<IPlugin> GetLoadedPlugins()
    {
        return _loadedPlugins.AsReadOnly();
    }
}
