using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RVR.Framework.Core.Modules;

public static class KbaModuleExtensions
{
    private static readonly List<IRvrModule> _modules = new();

    /// <summary>
    /// Registers RVR modules explicitly (AOT-compatible).
    /// Pass module instances directly instead of relying on assembly scanning.
    /// </summary>
    public static IServiceCollection AddRvrModules(this IServiceCollection services, IConfiguration configuration, params IRvrModule[] modules)
    {
        foreach (var module in modules)
        {
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
