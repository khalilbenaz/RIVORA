namespace RVR.Framework.EventSourcing.Extensions;

using Microsoft.Extensions.DependencyInjection;
using RVR.Framework.EventSourcing.Abstractions;
using RVR.Framework.EventSourcing.Stores;

/// <summary>
/// Extension methods for registering event sourcing services.
/// </summary>
public static class EventSourcingServiceCollectionExtensions
{
    /// <summary>
    /// Adds RVR Event Sourcing services with the in-memory event store.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRvrEventSourcing(this IServiceCollection services)
    {
        services.AddSingleton<IEventStore, InMemoryEventStore>();
        return services;
    }

    /// <summary>
    /// Adds RVR Event Sourcing services with a custom event store implementation.
    /// </summary>
    /// <typeparam name="TStore">The event store implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRvrEventSourcing<TStore>(this IServiceCollection services)
        where TStore : class, IEventStore
    {
        services.AddSingleton<IEventStore, TStore>();
        return services;
    }
}
