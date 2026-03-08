namespace KBA.Framework.Security.Extensions;

using System;
using KBA.Framework.Security.Attributes;
using KBA.Framework.Security.Interceptors;
using KBA.Framework.Security.Interfaces;
using KBA.Framework.Security.Jobs;
using KBA.Framework.Security.Middleware;
using KBA.Framework.Security.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering KBA Framework Security services.
/// </summary>
public static class SecurityServiceCollectionExtensions
{
    /// <summary>
    /// Adds all KBA Framework Security services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure security options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddKbaSecurity(
        this IServiceCollection services,
        Action<SecurityOptions>? configureOptions = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddOptions<SecurityOptions>()
            .Configure(configureOptions ?? (_ => { }));

        // Refresh Token Services
        services.AddSingleton<IRefreshTokenStore, InMemoryRefreshTokenStore>();
        services.AddSingleton<IRefreshTokenService, RefreshTokenService>();
        services.AddHostedService<RefreshTokenCleanupJob>();

        // Rate Limiting Services
        services.AddSingleton<IRateLimitStore, InMemoryRateLimitStore>();
        services.AddSingleton<IRateLimitService, RateLimitService>();

        // Audit Services
        services.AddSingleton<IAuditStore, InMemoryAuditStore>();
        services.AddSingleton<IAuditService, AuditService>();
        services.AddSingleton<SaveChangesInterceptor, AuditTrailInterceptor>();

        // TOTP Services
        services.AddSingleton<IBackupCodeStore, InMemoryBackupCodeStore>();
        services.AddSingleton<ITotpService, TotpService>();

        // Permission Services
        services.AddSingleton<IPermissionStore, InMemoryPermissionStore>();
        services.AddSingleton<IPermissionService, PermissionService>();

        // Authorization
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        // HTTP Context accessor for audit trail
        services.AddHttpContextAccessor();

        return services;
    }

    /// <summary>
    /// Adds KBA Framework Security services with Redis stores for distributed deployments.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="redisConnectionString">The Redis connection string.</param>
    /// <param name="configureOptions">Action to configure security options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddKbaSecurityWithRedis(
        this IServiceCollection services,
        string redisConnectionString,
        Action<SecurityOptions>? configureOptions = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            throw new ArgumentException("Redis connection string cannot be empty.", nameof(redisConnectionString));
        }

        // Add Redis connection
        services.AddSingleton(sp =>
            StackExchange.Redis.ConnectionMultiplexer.Connect(redisConnectionString));

        // Refresh Token Services (use EF Core store for persistence)
        services.AddSingleton<IRefreshTokenStore, InMemoryRefreshTokenStore>();
        services.AddSingleton<IRefreshTokenService, RefreshTokenService>();
        services.AddHostedService<RefreshTokenCleanupJob>();

        // Rate Limiting Services (Redis)
        services.AddSingleton<IRateLimitStore, RedisRateLimitStore>();
        services.AddSingleton<IRateLimitService, RateLimitService>();

        // Audit Services
        services.AddSingleton<IAuditStore, InMemoryAuditStore>();
        services.AddSingleton<IAuditService, AuditService>();
        services.AddSingleton<SaveChangesInterceptor, AuditTrailInterceptor>();

        // TOTP Services
        services.AddSingleton<IBackupCodeStore, InMemoryBackupCodeStore>();
        services.AddSingleton<ITotpService, TotpService>();

        // Permission Services
        services.AddSingleton<IPermissionStore, InMemoryPermissionStore>();
        services.AddSingleton<IPermissionService, PermissionService>();

        // Authorization
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        // HTTP Context accessor for audit trail
        services.AddHttpContextAccessor();

        return services;
    }

    /// <summary>
    /// Adds only the refresh token services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure refresh token options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRefreshTokenServices(
        this IServiceCollection services,
        Action<RefreshTokenOptions>? configureOptions = null)
    {
        services.AddOptions<RefreshTokenOptions>()
            .Configure(configureOptions ?? (_ => { }));

        services.AddSingleton<IRefreshTokenStore, InMemoryRefreshTokenStore>();
        services.AddSingleton<IRefreshTokenService, RefreshTokenService>();
        services.AddHostedService<RefreshTokenCleanupJob>();

        return services;
    }

    /// <summary>
    /// Adds only the rate limiting services and middleware.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure rate limit options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRateLimitingServices(
        this IServiceCollection services,
        Action<RateLimitOptions>? configureOptions = null)
    {
        services.AddOptions<RateLimitOptions>()
            .Configure(configureOptions ?? (_ => { }));

        services.AddSingleton<IRateLimitStore, InMemoryRateLimitStore>();
        services.AddSingleton<IRateLimitService, RateLimitService>();

        return services;
    }

    /// <summary>
    /// Adds only the audit trail services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure audit options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddAuditTrailServices(
        this IServiceCollection services,
        Action<AuditTrailInterceptorOptions>? configureOptions = null)
    {
        services.AddOptions<AuditTrailInterceptorOptions>()
            .Configure(configureOptions ?? (_ => { }));

        services.AddSingleton<IAuditStore, InMemoryAuditStore>();
        services.AddSingleton<IAuditService, AuditService>();
        services.AddSingleton<SaveChangesInterceptor, AuditTrailInterceptor>();
        services.AddHttpContextAccessor();

        return services;
    }

    /// <summary>
    /// Adds only the TOTP/2FA services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure TOTP options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddTotpServices(
        this IServiceCollection services,
        Action<TotpOptions>? configureOptions = null)
    {
        services.AddOptions<TotpOptions>()
            .Configure(configureOptions ?? (_ => { }));

        services.AddSingleton<IBackupCodeStore, InMemoryBackupCodeStore>();
        services.AddSingleton<ITotpService, TotpService>();

        return services;
    }

    /// <summary>
    /// Adds only the permission/RBAC services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure permission options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddPermissionServices(
        this IServiceCollection services,
        Action<PermissionOptions>? configureOptions = null)
    {
        services.AddOptions<PermissionOptions>()
            .Configure(configureOptions ?? (_ => { }));

        services.AddSingleton<IPermissionStore, InMemoryPermissionStore>();
        services.AddSingleton<IPermissionService, PermissionService>();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        return services;
    }
}

/// <summary>
/// Configuration options for KBA Framework Security.
/// </summary>
public class SecurityOptions
{
    /// <summary>
    /// Gets or sets the refresh token options.
    /// </summary>
    public RefreshTokenOptions RefreshToken { get; set; } = new();

    /// <summary>
    /// Gets or sets the rate limit options.
    /// </summary>
    public RateLimitOptions RateLimit { get; set; } = new();

    /// <summary>
    /// Gets or sets the audit trail options.
    /// </summary>
    public AuditTrailInterceptorOptions AuditTrail { get; set; } = new();

    /// <summary>
    /// Gets or sets the TOTP options.
    /// </summary>
    public TotpOptions Totp { get; set; } = new();

    /// <summary>
    /// Gets or sets the permission options.
    /// </summary>
    public PermissionOptions Permission { get; set; } = new();
}

/// <summary>
/// Configuration options for permissions.
/// </summary>
public class PermissionOptions
{
    /// <summary>
    /// Gets or sets the list of permission definitions to register.
    /// </summary>
    public List<PermissionDefinition> Permissions { get; set; } = new();

    /// <summary>
    /// Gets or sets whether to enable hierarchical permissions.
    /// </summary>
    public bool EnableHierarchicalPermissions { get; set; } = false;
}
