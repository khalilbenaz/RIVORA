using RVR.Framework.Core;
using RVR.Framework.Core.Security;
using RVR.Framework.Infrastructure.Configuration;
using RVR.Framework.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace RVR.Framework.Infrastructure.Extensions;

/// <summary>
/// Extensions pour IServiceCollection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configure la base de données avec les paramètres optimisés
    /// </summary>
    public static IServiceCollection AddOptimizedDbContext(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Charger la chaîne de connexion depuis ConnectionStrings
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\mssqllocaldb;Database=RVRFrameworkDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true";

        // Charger les configurations de la base de données
        var dbSection = configuration.GetSection("DatabaseSettings");
        var dbSettings = new DatabaseSettings
        {
            ConnectionString = connectionString,
            CommandTimeout = int.Parse(dbSection["CommandTimeout"] ?? "30"),
            EnableRetryOnFailure = bool.Parse(dbSection["EnableRetryOnFailure"] ?? "true"),
            MaxRetryCount = int.Parse(dbSection["MaxRetryCount"] ?? "3"),
            MaxRetryDelay = TimeSpan.Parse(dbSection["MaxRetryDelay"] ?? "00:00:05"),
            EnableSensitiveDataLogging = bool.Parse(dbSection["EnableSensitiveDataLogging"] ?? "false"),
            EnableDetailedErrors = bool.Parse(dbSection["EnableDetailedErrors"] ?? "false"),
            MigrationsAssembly = dbSection["MigrationsAssembly"] ?? "RVR.Framework.Infrastructure"
        };

        var efSection = configuration.GetSection("EntityFramework");
        var poolSize = int.Parse(efSection["MaxPoolSize"] ?? "128");
        var efSettings = new EntityFrameworkSettings
        {
            UseNoTracking = bool.Parse(efSection["UseNoTracking"] ?? "true"),
            EnableLazyLoading = bool.Parse(efSection["EnableLazyLoading"] ?? "false")
        };

        services.AddDbContext<RVRDbContext>((serviceProvider, options) =>
        {
            var tenantProvider = serviceProvider.GetRequiredService<RVR.Framework.MultiTenancy.ITenantProvider>();
            var activeConnectionString = tenantProvider.GetConnectionString() ?? dbSettings.ConnectionString;

            // Configuration de base
            options.UseSqlServer(
                activeConnectionString,
                sqlOptions =>
                {
                    // Assembly de migrations
                    sqlOptions.MigrationsAssembly(dbSettings.MigrationsAssembly);

                    // Retry on failure
                    if (dbSettings.EnableRetryOnFailure)
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: dbSettings.MaxRetryCount,
                            maxRetryDelay: dbSettings.MaxRetryDelay,
                            errorNumbersToAdd: null);
                    }

                    // Command timeout
                    sqlOptions.CommandTimeout(dbSettings.CommandTimeout);
                });

            // Query splitting pour optimiser les performances (disponible uniquement avec EF Core 5+)
            // Note: Cette fonctionnalité peut être activée au niveau des requêtes individuelles si nécessaire

            // Logging et détails
            if (dbSettings.EnableSensitiveDataLogging)
            {
                options.EnableSensitiveDataLogging();
            }

            if (dbSettings.EnableDetailedErrors)
            {
                options.EnableDetailedErrors();
            }

            // Le lazy loading est désactivé par défaut dans EF Core
            // Note: Pour l'activer, il faudrait installer Microsoft.EntityFrameworkCore.Proxies

            // Configuration du tracking par défaut
            if (efSettings.UseNoTracking)
            {
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }
        });

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    /// <summary>
    /// Registers a <see cref="ConfigurationEncryptionKeyProvider"/> that reads the AES-256 key
    /// from <c>Security:EncryptionKey</c> in <see cref="IConfiguration"/>.
    /// </summary>
    public static IServiceCollection AddEncryptionKeyProvider(this IServiceCollection services)
    {
        services.AddSingleton<IEncryptionKeyProvider, ConfigurationEncryptionKeyProvider>();
        return services;
    }

    /// <summary>
    /// Registers the <see cref="EnvironmentEncryptionKeyProvider"/> that reads the AES-256 key
    /// from the <c>RVR_ENCRYPTION_KEY</c> environment variable.
    /// </summary>
    public static IServiceCollection AddEncryptionKeyProviderFromEnvironment(this IServiceCollection services)
    {
        services.AddSingleton<IEncryptionKeyProvider, EnvironmentEncryptionKeyProvider>();
        return services;
    }

    /// <summary>
    /// Registers a custom <see cref="IEncryptionKeyProvider"/> implementation.
    /// </summary>
    public static IServiceCollection AddEncryptionKeyProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, IEncryptionKeyProvider
    {
        services.AddSingleton<IEncryptionKeyProvider, TProvider>();
        return services;
    }
}
