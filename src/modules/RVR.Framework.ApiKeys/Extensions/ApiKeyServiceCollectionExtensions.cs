using Microsoft.Extensions.DependencyInjection;
using RVR.Framework.ApiKeys.Services;

namespace RVR.Framework.ApiKeys.Extensions;

/// <summary>
/// Extension methods for registering API key services.
/// </summary>
public static class ApiKeyServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Rivora API key management services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRvrApiKeys(this IServiceCollection services)
    {
        services.AddSingleton<IApiKeyService, ApiKeyService>();
        return services;
    }
}
