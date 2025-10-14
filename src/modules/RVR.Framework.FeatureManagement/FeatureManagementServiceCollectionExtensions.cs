using RVR.Framework.FeatureManagement.Services;
using Microsoft.Extensions.DependencyInjection;

namespace RVR.Framework.FeatureManagement;

public static class FeatureManagementServiceCollectionExtensions
{
    /// <summary>
    /// Ajoute les services de Feature Management au conteneur DI.
    /// </summary>
    public static IServiceCollection AddRvrFeatureManagement(this IServiceCollection services)
    {
        services.AddSingleton<IFeatureDefinitionProvider, InMemoryFeatureDefinitionProvider>();
        services.AddScoped<IFeatureCheckerService, FeatureCheckerService>();
        return services;
    }

    /// <summary>
    /// Ajoute les services de Feature Management avec configuration personnalisee.
    /// </summary>
    public static IServiceCollection AddRvrFeatureManagement(
        this IServiceCollection services,
        Action<FeatureManagementOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddRvrFeatureManagement();
        return services;
    }
}

/// <summary>
/// Options de configuration pour le Feature Management.
/// </summary>
public class FeatureManagementOptions
{
    /// <summary>
    /// Definitions de features par defaut.
    /// </summary>
    public List<FeatureRegistration> Features { get; set; } = new();
}

/// <summary>
/// Enregistrement d'une feature avec sa valeur par defaut.
/// </summary>
public class FeatureRegistration
{
    public string Name { get; set; } = string.Empty;
    public string DefaultValue { get; set; } = string.Empty;
    public string ValueType { get; set; } = "Boolean";
}
