using Microsoft.Extensions.DependencyInjection;
using RVR.Framework.Messaging.Services;

namespace RVR.Framework.Messaging.Extensions;

/// <summary>
/// Extension methods for registering messaging services.
/// </summary>
public static class MessagingServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Rivora in-memory messaging services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRvrMessaging(this IServiceCollection services)
    {
        services.AddSingleton<IMessageBus, InMemoryMessageBus>();
        return services;
    }
}
