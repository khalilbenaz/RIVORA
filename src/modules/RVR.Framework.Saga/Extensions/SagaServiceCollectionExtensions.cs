namespace RVR.Framework.Saga.Extensions;

using Microsoft.Extensions.DependencyInjection;
using RVR.Framework.Saga.Abstractions;
using RVR.Framework.Saga.Orchestration;
using RVR.Framework.Saga.Stores;

/// <summary>
/// Extension methods for registering saga services.
/// </summary>
public static class SagaServiceCollectionExtensions
{
    /// <summary>
    /// Adds RVR Saga / Process Manager services with the in-memory saga store.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRvrSagas(this IServiceCollection services)
    {
        services.AddSingleton<ISagaStore, InMemorySagaStore>();
        services.AddScoped<SagaOrchestrator>();
        return services;
    }

    /// <summary>
    /// Adds RVR Saga / Process Manager services with a custom saga store implementation.
    /// </summary>
    /// <typeparam name="TStore">The saga store implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRvrSagas<TStore>(this IServiceCollection services)
        where TStore : class, ISagaStore
    {
        services.AddSingleton<ISagaStore, TStore>();
        services.AddScoped<SagaOrchestrator>();
        return services;
    }

    /// <summary>
    /// Registers a saga handler in the dependency injection container.
    /// </summary>
    /// <typeparam name="TSaga">The saga implementation type.</typeparam>
    /// <typeparam name="TData">The saga data type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSaga<TSaga, TData>(this IServiceCollection services)
        where TSaga : class, ISaga<TData>
        where TData : class, new()
    {
        services.AddScoped<ISaga<TData>, TSaga>();
        return services;
    }
}
