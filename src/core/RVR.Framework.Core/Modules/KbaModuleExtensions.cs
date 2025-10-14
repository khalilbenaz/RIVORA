using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RVR.Framework.Core.Modules;

public static class KbaModuleExtensions
{
    private static readonly List<IRvrModule> _modules = new();

    /// <summary>
    /// Découvre et enregistre tous les modules RVR dans les assemblies spécifiés
    /// </summary>
    public static IServiceCollection AddRvrModules(this IServiceCollection services, IConfiguration configuration, params Assembly[] assemblies)
    {
        var moduleTypes = assemblies.SelectMany(a => a.GetTypes())
            .Where(t => typeof(IRvrModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in moduleTypes)
        {
            var module = (IRvrModule)Activator.CreateInstance(type)!;
            module.ConfigureServices(services, configuration);
            _modules.Add(module);
        }

        return services;
    }

    /// <summary>
    /// Configure le pipeline pour tous les modules enregistrés
    /// </summary>
    public static IApplicationBuilder UseRvrModules(this IApplicationBuilder app)
    {
        foreach (var module in _modules)
        {
            module.Configure(app);
        }

        return app;
    }
}
