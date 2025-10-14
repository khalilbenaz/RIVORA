namespace RVR.Framework.Client.Extensions;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Deprecated: Use <see cref="RivoraClientServiceCollectionExtensions"/> instead.
/// </summary>
[System.Obsolete("Use RivoraClientServiceCollectionExtensions instead. This class will be removed in a future version.")]
public static class KbaClientServiceCollectionExtensions
{
    /// <summary>
    /// Deprecated: Use <see cref="RivoraClientServiceCollectionExtensions.AddRivoraApiClient"/> instead.
    /// </summary>
    [System.Obsolete("Use AddRivoraApiClient instead. This method will be removed in a future version.")]
    public static IServiceCollection AddRvrApiClient(this IServiceCollection services, string baseUrl)
    {
        return RivoraClientServiceCollectionExtensions.AddRivoraApiClient(services, baseUrl);
    }
}
