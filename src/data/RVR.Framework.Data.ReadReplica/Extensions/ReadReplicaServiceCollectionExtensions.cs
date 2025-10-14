namespace RVR.Framework.Data.ReadReplica.Extensions;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering read-replica services.
/// </summary>
public static class ReadReplicaServiceCollectionExtensions
{
    /// <summary>
    /// Adds multi-database read replica routing to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">An action to configure <see cref="ReadReplicaOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRvrReadReplicas(
        this IServiceCollection services,
        Action<ReadReplicaOptions> configure)
    {
        services.Configure(configure);
        services.AddSingleton<IDbConnectionRouter, RoundRobinConnectionRouter>();
        services.AddSingleton(typeof(ReadReplicaDbContextFactory<>));
        return services;
    }

    /// <summary>
    /// Adds multi-database read replica routing with a custom router implementation.
    /// </summary>
    /// <typeparam name="TRouter">The custom router type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">An action to configure <see cref="ReadReplicaOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRvrReadReplicas<TRouter>(
        this IServiceCollection services,
        Action<ReadReplicaOptions> configure)
        where TRouter : class, IDbConnectionRouter
    {
        services.Configure(configure);
        services.AddSingleton<IDbConnectionRouter, TRouter>();
        services.AddSingleton(typeof(ReadReplicaDbContextFactory<>));
        return services;
    }
}
