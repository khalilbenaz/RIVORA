namespace RVR.Framework.Features.Core;

/// <summary>
/// Enumerates the available feature flag providers.
/// </summary>
public enum FeatureProvider
{
    /// <summary>
    /// Configuration-based provider (appsettings.json, environment variables, etc.).
    /// </summary>
    Config = 0,

    /// <summary>
    /// Database-based provider using Entity Framework Core.
    /// </summary>
    Database = 1,

    /// <summary>
    /// Azure App Configuration provider.
    /// </summary>
    Azure = 2
}
