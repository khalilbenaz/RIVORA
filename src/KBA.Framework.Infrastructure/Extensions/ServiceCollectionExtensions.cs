using KBA.Framework.Infrastructure.Configuration;
using KBA.Framework.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace KBA.Framework.Infrastructure.Extensions;

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
            ?? "Server=(localdb)\\mssqllocaldb;Database=KBAFrameworkDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true";

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
            MigrationsAssembly = dbSection["MigrationsAssembly"] ?? "KBA.Framework.Infrastructure"
        };

        var efSection = configuration.GetSection("EntityFramework");
        var poolSize = int.Parse(efSection["MaxPoolSize"] ?? "128");
        var efSettings = new EntityFrameworkSettings
        {
            UseNoTracking = bool.Parse(efSection["UseNoTracking"] ?? "true"),
            EnableLazyLoading = bool.Parse(efSection["EnableLazyLoading"] ?? "false")
        };

        services.AddDbContextPool<KBADbContext>((serviceProvider, options) =>
        {
            // Configuration de base
            options.UseSqlServer(
                dbSettings.ConnectionString,
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

        return services;
    }
}
