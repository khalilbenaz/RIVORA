using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KBA.Framework.Core.Modules;

public static class KbaModuleExtensions
{
    private static readonly List<IKbaModule> _modules = new();

    /// <summary>
    /// Découvre et enregistre tous les modules KBA dans les assemblies spécifiés
    /// </summary>
    public static IServiceCollection AddKbaModules(this IServiceCollection services, IConfiguration configuration, params Assembly[] assemblies)
    {
        var moduleTypes = assemblies.SelectMany(a => a.GetTypes())
            .Where(t => typeof(IKbaModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in moduleTypes)
        {
            var module = (IKbaModule)Activator.CreateInstance(type)!;
            module.ConfigureServices(services, configuration);
            _modules.Add(module);
        }

        return services;
    }

    /// <summary>
    /// Configure le pipeline pour tous les modules enregistrés
    /// </summary>
    public static IApplicationBuilder UseKbaModules(this IApplicationBuilder app)
    {
        foreach (var module in _modules)
        {
            module.Configure(app);
        }

        return app;
    }
}
