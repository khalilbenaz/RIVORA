namespace RVR.Framework.Privacy.Extensions;

using Microsoft.Extensions.DependencyInjection;
using RVR.Framework.Privacy.Services;

/// <summary>
/// Extension methods for registering RIVORA Framework Privacy / GDPR compliance services.
/// </summary>
public static class PrivacyServiceCollectionExtensions
{
    /// <summary>
    /// Adds all RIVORA Framework Privacy and GDPR compliance services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRvrPrivacy(this IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        // Consent store
        services.AddSingleton<InMemoryConsentStore>();

        // Data anonymizer
        services.AddSingleton<IDataAnonymizer, DataAnonymizer>();

        // Privacy service
        services.AddSingleton<IPrivacyService, PrivacyService>();

        return services;
    }
}
