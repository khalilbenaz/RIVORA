using Microsoft.Extensions.DependencyInjection;

namespace RVR.Framework.SaaS.Analytics.Extensions;

/// <summary>
/// Extension methods for registering cross-tenant analytics services
/// in the dependency injection container.
/// </summary>
public static class AnalyticsServiceCollectionExtensions
{
    /// <summary>
    /// Adds cross-tenant analytics services to the service collection.
    /// Registers <see cref="ICrossTenantAnalyticsService"/> with a default
    /// <see cref="CrossTenantAnalyticsService"/> implementation that uses
    /// in-memory seed data. For production, replace the implementation with
    /// a version backed by a persistent data store.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCrossTenantAnalytics(this IServiceCollection services)
    {
        services.AddSingleton<ICrossTenantAnalyticsService, CrossTenantAnalyticsService>();
        return services;
    }

    /// <summary>
    /// Adds cross-tenant analytics services with a custom implementation.
    /// </summary>
    /// <typeparam name="TImplementation">
    /// The concrete type implementing <see cref="ICrossTenantAnalyticsService"/>.
    /// </typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCrossTenantAnalytics<TImplementation>(
        this IServiceCollection services)
        where TImplementation : class, ICrossTenantAnalyticsService
    {
        services.AddSingleton<ICrossTenantAnalyticsService, TImplementation>();
        return services;
    }
}
