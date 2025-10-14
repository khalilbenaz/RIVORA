namespace RVR.Framework.Security.Extensions;

using System;
using RVR.Framework.Security.Attributes;
using RVR.Framework.Security.Interceptors;
using RVR.Framework.Security.Interfaces;
using RVR.Framework.Security.Jobs;
using RVR.Framework.Security.Middleware;
using RVR.Framework.Security.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering RIVORA Framework Security services.
/// </summary>
public static class SecurityServiceCollectionExtensions
{
    /// <summary>
    /// Adds all RIVORA Framework Security services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure security options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRvrSecurity(
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
        services.AddSingleton<EndpointRuleCache>();

        // Audit Services
        services.AddSingleton(new AuditTrailInterceptorOptions());
        services.AddSingleton<IAuditStore, InMemoryAuditStore>();
        services.AddSingleton<IAuditService, AuditService>();
        services.AddSingleton<SaveChangesInterceptor, AuditTrailInterceptor>();

        // TOTP Services
        services.AddSingleton<IBackupCodeStore, InMemoryBackupCodeStore>();
        services.AddSingleton<ITotpService, TotpService>();

        // Password Hashing Services
        services.AddOptions<PasswordHasherOptions>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

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
    /// Adds RIVORA Framework Security services with Redis stores for distributed deployments.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="redisConnectionString">The Redis connection string.</param>
    /// <param name="configureOptions">Action to configure security options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRvrSecurityWithRedis(
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
        services.AddSingleton<EndpointRuleCache>();

        // Audit Services
        services.AddSingleton(new AuditTrailInterceptorOptions());
        services.AddSingleton<IAuditStore, InMemoryAuditStore>();
        services.AddSingleton<IAuditService, AuditService>();
        services.AddSingleton<SaveChangesInterceptor, AuditTrailInterceptor>();

        // TOTP Services
        services.AddSingleton<IBackupCodeStore, InMemoryBackupCodeStore>();
        services.AddSingleton<ITotpService, TotpService>();

        // Password Hashing Services
        services.AddOptions<PasswordHasherOptions>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

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
        services.AddSingleton<EndpointRuleCache>();

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

    /// <summary>
    /// Adds external authentication (SSO) support.
    /// </summary>
    public static IServiceCollection AddRvrExternalAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection("Security:ExternalAuth");
        var options = section.Get<ExternalProviderOptions>();

        if (options == null || !options.IsEnabled)
        {
            return services;
        }

        services.AddAuthentication()
            .AddOpenIdConnect("OpenIdConnect", opt =>
            {
                opt.Authority = options.Authority;
                opt.ClientId = options.ClientId;
                opt.ClientSecret = options.ClientSecret;
                opt.ResponseType = "code";
                opt.CallbackPath = options.CallbackPath;
                opt.SaveTokens = true;

                foreach (var scope in options.Scopes)
                {
                    opt.Scope.Add(scope);
                }

                opt.GetClaimsFromUserInfoEndpoint = true;
            });

        return services;
    }

    /// <summary>
    /// Adds only the password hashing services with configurable options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure password hasher options (e.g., work factor).</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddPasswordHashingServices(
        this IServiceCollection services,
        Action<PasswordHasherOptions>? configureOptions = null)
    {
        services.AddOptions<PasswordHasherOptions>()
            .Configure(configureOptions ?? (_ => { }));

        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        return services;
    }
}

/// <summary>
/// Configuration options for RIVORA Framework Security.
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

    /// <summary>
    /// Gets or sets the external authentication provider options.
    /// </summary>
    public ExternalProviderOptions ExternalAuth { get; set; } = new();
}

/// <summary>
/// Configuration options for external authentication providers.
/// </summary>
public class ExternalProviderOptions
{
    public bool IsEnabled { get; set; } = false;
    public string Authority { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public List<string> Scopes { get; set; } = new() { "openid", "profile", "email" };
    public string CallbackPath { get; set; } = "/signin-oidc";
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
