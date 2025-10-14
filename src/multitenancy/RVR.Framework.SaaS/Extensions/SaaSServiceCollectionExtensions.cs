using Microsoft.Extensions.DependencyInjection;
using RVR.Framework.SaaS.Services;

namespace RVR.Framework.SaaS.Extensions;

/// <summary>
/// Extension methods for registering SaaS tenant lifecycle services.
/// </summary>
public static class SaaSServiceCollectionExtensions
{
    /// <summary>
    /// Adds Rivora SaaS tenant lifecycle services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRvrSaaS(this IServiceCollection services)
    {
        services.AddSingleton<ITenantLifecycleService, TenantLifecycleService>();
        return services;
    }
}
