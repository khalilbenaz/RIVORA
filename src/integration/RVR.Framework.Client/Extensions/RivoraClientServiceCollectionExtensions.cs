namespace RVR.Framework.Client.Extensions;

using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering <see cref="RivoraApiClient"/> in the DI container.
/// </summary>
public static class RivoraClientServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="RivoraApiClient"/> as a typed HTTP client targeting <paramref name="baseUrl"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="baseUrl">The base URL of the RIVORA Framework API.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddRivoraApiClient(this IServiceCollection services, string baseUrl)
    {
        services.AddHttpClient<RivoraApiClient>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });
        return services;
    }
}
