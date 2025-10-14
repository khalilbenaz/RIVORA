namespace RVR.Framework.Search.Extensions;

using System;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.DependencyInjection;
using RVR.Framework.Search.Interfaces;
using RVR.Framework.Search.Models;
using RVR.Framework.Search.Services;

/// <summary>
/// Extension methods for configuring RIVORA Framework Search services.
/// </summary>
public static class SearchServiceCollectionExtensions
{
    /// <summary>
    /// Adds RIVORA Framework Search services with Elasticsearch implementation.
    /// </summary>
    /// <typeparam name="T">The type of the documents being searched.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure search options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRvrSearch<T>(
        this IServiceCollection services,
        Action<SearchOptions>? configureOptions = null) where T : class
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        var options = new SearchOptions();
        configureOptions?.Invoke(options);

        services.AddOptions<SearchOptions>()
            .Configure(configureOptions ?? (_ => { }));

        // Register Elasticsearch client
        services.AddSingleton(_ =>
        {
            var settings = new ElasticsearchClientSettings(new Uri(options.NodeUri))
                .DefaultIndex(options.Index);

            return new ElasticsearchClient(settings);
        });

        services.AddSingleton<ISearchService<T>, ElasticsearchSearchService<T>>();

        return services;
    }

    /// <summary>
    /// Adds RIVORA Framework Search services with in-memory implementation (for dev/testing).
    /// </summary>
    /// <typeparam name="T">The type of the documents being searched.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRvrSearchInMemory<T>(
        this IServiceCollection services) where T : class
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddSingleton<ISearchService<T>, InMemorySearchService<T>>();

        return services;
    }
}
