namespace KBA.Framework.Caching.Extensions;

using System;
using KBA.Framework.Caching.Interfaces;
using KBA.Framework.Caching.Middleware;
using KBA.Framework.Caching.Models;
using KBA.Framework.Caching.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring KBA Framework Caching.
/// </summary>
public static class CachingServiceCollectionExtensions
{
    /// <summary>
    /// Adds all KBA Framework Caching services with in-memory implementation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure cache options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddKbaCaching(
        this IServiceCollection services,
        Action<CacheOptions>? configureOptions = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddOptions<CacheOptions>()
            .Configure(configureOptions ?? (_ => { }));

        services.AddSingleton<IResponseCacheService, MemoryResponseCacheService>();

        return services;
    }

    /// <summary>
    /// Adds KBA Framework Caching services with Redis implementation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="redisConnectionString">The Redis connection string.</param>
    /// <param name="configureOptions">Action to configure cache options.</param>
    /// <param name="configureRedisOptions">Action to configure Redis options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddKbaCachingWithRedis(
        this IServiceCollection services,
        string redisConnectionString,
        Action<CacheOptions>? configureOptions = null,
        Action<RedisCacheOptions>? configureRedisOptions = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            throw new ArgumentException("Redis connection string cannot be empty.", nameof(redisConnectionString));
        }

        services.AddOptions<CacheOptions>()
            .Configure(configureOptions ?? (_ => { }));

        services.AddOptions<RedisCacheOptions>()
            .Configure(options =>
            {
                options.ConnectionString = redisConnectionString;
                configureRedisOptions?.Invoke(options);
            });

        // Add Redis connection multiplexer
        services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp =>
        {
            var redisOptions = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RedisCacheOptions>>().Value;
            var configurationOptions = StackExchange.Redis.ConfigurationOptions.Parse(redisOptions.ConnectionString);
            configurationOptions.Ssl = redisOptions.Ssl;
            configurationOptions.Password = redisOptions.Password;
            configurationOptions.ConnectTimeout = redisOptions.ConnectTimeout;
            configurationOptions.SyncTimeout = redisOptions.SyncTimeout;
            configurationOptions.AsyncTimeout = redisOptions.AsyncTimeout;
            configurationOptions.AllowAdmin = redisOptions.AllowAdmin;
            configurationOptions.AbortOnConnectFail = redisOptions.AbortOnConnectFail;
            configurationOptions.KeepAlive = redisOptions.KeepAlive;
            configurationOptions.WriteBuffer = redisOptions.WriteBuffer;

            if (!string.IsNullOrWhiteSpace(redisOptions.TieBreaker))
            {
                configurationOptions.TieBreaker = redisOptions.TieBreaker;
            }

            return StackExchange.Redis.ConnectionMultiplexer.Connect(configurationOptions);
        });

        services.AddSingleton<IResponseCacheService, RedisResponseCacheService>();

        return services;
    }

    /// <summary>
    /// Adds only the memory cache service.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure cache options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddKbaMemoryCache(
        this IServiceCollection services,
        Action<CacheOptions>? configureOptions = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddOptions<CacheOptions>()
            .Configure(configureOptions ?? (_ => { }));

        services.AddSingleton<IResponseCacheService, MemoryResponseCacheService>();

        return services;
    }

    /// <summary>
    /// Adds only the Redis cache service.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="redisConnectionString">The Redis connection string.</param>
    /// <param name="configureOptions">Action to configure cache options.</param>
    /// <param name="configureRedisOptions">Action to configure Redis options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddKbaRedisCache(
        this IServiceCollection services,
        string redisConnectionString,
        Action<CacheOptions>? configureOptions = null,
        Action<RedisCacheOptions>? configureRedisOptions = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            throw new ArgumentException("Redis connection string cannot be empty.", nameof(redisConnectionString));
        }

        services.AddOptions<CacheOptions>()
            .Configure(configureOptions ?? (_ => { }));

        services.AddOptions<RedisCacheOptions>()
            .Configure(options =>
            {
                options.ConnectionString = redisConnectionString;
                configureRedisOptions?.Invoke(options);
            });

        services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp =>
        {
            var redisOptions = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RedisCacheOptions>>().Value;
            return StackExchange.Redis.ConnectionMultiplexer.Connect(redisOptions.ConnectionString);
        });

        services.AddSingleton<IResponseCacheService, RedisResponseCacheService>();

        return services;
    }

    /// <summary>
    /// Adds the standard .NET memory caching services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure memory cache options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddKbaMemoryCacheServices(
        this IServiceCollection services,
        Action<MemoryCacheOptions>? configureOptions = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddMemoryCache(configureOptions);

        return services;
    }

    /// <summary>
    /// Adds the distributed caching services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="redisConnectionString">The Redis connection string.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddKbaDistributedCache(
        this IServiceCollection services,
        string redisConnectionString)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
        });

        return services;
    }

    /// <summary>
    /// Configures the response caching middleware.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder.</returns>
    public static IApplicationBuilder UseKbaResponseCaching(this IApplicationBuilder app)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        app.UseMiddleware<ResponseCachingMiddleware>();

        return app;
    }
}

/// <summary>
/// Helper class for cache operations.
/// </summary>
public static class CacheHelper
{
    /// <summary>
    /// Generates a cache key from the specified components.
    /// </summary>
    /// <param name="components">The key components.</param>
    /// <returns>The generated cache key.</returns>
    public static string GenerateKey(params string[] components)
    {
        if (components == null || components.Length == 0)
        {
            return string.Empty;
        }

        return string.Join(":", components.Where(c => !string.IsNullOrWhiteSpace(c)));
    }

    /// <summary>
    /// Generates a cache key with a prefix.
    /// </summary>
    /// <param name="prefix">The key prefix.</param>
    /// <param name="components">The key components.</param>
    /// <returns>The generated cache key.</returns>
    public static string GenerateKeyWithPrefix(string prefix, params string[] components)
    {
        var key = GenerateKey(components);
        return string.IsNullOrWhiteSpace(prefix) ? key : $"{prefix}:{key}";
    }

    /// <summary>
    /// Invalidates cache entries by tag.
    /// </summary>
    /// <param name="cacheService">The cache service.</param>
    /// <param name="tags">The tags to invalidate.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async System.Threading.Tasks.Task InvalidateTagsAsync(
        this IResponseCacheService cacheService,
        params string[] tags)
    {
        if (cacheService == null)
        {
            throw new ArgumentNullException(nameof(cacheService));
        }

        await cacheService.InvalidateByTagsAsync(tags);
    }
}
