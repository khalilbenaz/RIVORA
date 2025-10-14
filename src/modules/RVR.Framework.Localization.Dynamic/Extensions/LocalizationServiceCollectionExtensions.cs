using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using RVR.Framework.Localization.Dynamic.Services;

namespace RVR.Framework.Localization.Dynamic.Extensions;

/// <summary>
/// Extension methods for registering RVR dynamic localization services.
/// </summary>
public static class LocalizationServiceCollectionExtensions
{
    /// <summary>
    /// Adds RVR dynamic localization services to the service collection.
    /// Registers the <see cref="InMemoryLocalizationStore"/> as the default <see cref="ILocalizationStore"/>
    /// and <see cref="DatabaseStringLocalizer"/> as the <see cref="IStringLocalizer"/> implementation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRvrDynamicLocalization(this IServiceCollection services)
    {
        // Register the in-memory store as singleton by default.
        // Consumers can replace this with a database-backed implementation.
        services.TryAddSingleton<ILocalizationStore, InMemoryLocalizationStore>();

        // Register the string localizer
        services.TryAddTransient<IStringLocalizer, DatabaseStringLocalizer>();
        services.TryAddTransient(typeof(IStringLocalizer<>), typeof(DatabaseStringLocalizer<>));

        return services;
    }

    /// <summary>
    /// Adds RVR dynamic localization services with a custom <see cref="ILocalizationStore"/> implementation.
    /// </summary>
    /// <typeparam name="TStore">The custom localization store type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRvrDynamicLocalization<TStore>(this IServiceCollection services)
        where TStore : class, ILocalizationStore
    {
        services.AddSingleton<ILocalizationStore, TStore>();
        services.TryAddTransient<IStringLocalizer, DatabaseStringLocalizer>();
        services.TryAddTransient(typeof(IStringLocalizer<>), typeof(DatabaseStringLocalizer<>));

        return services;
    }
}
