namespace KBA.Framework.Data.Abstractions.Extensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Background service that automatically runs database migrations on application startup.
/// </summary>
public class AutoMigrationHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DatabaseOptions _options;
    private readonly ILogger<AutoMigrationHostedService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoMigrationHostedService"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="options">The database options.</param>
    /// <param name="logger">The logger.</param>
    public AutoMigrationHostedService(
        IServiceProvider serviceProvider,
        DatabaseOptions options,
        ILogger<AutoMigrationHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.AutoMigrate)
        {
            _logger.LogInformation("Auto-migration is disabled. Skipping database migration.");
            return;
        }

        _logger.LogInformation("Starting automatic database migration...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContexts = scope.ServiceProvider.GetServices<DbContext>();

            foreach (var dbContext in dbContexts)
            {
                var contextType = dbContext.GetType();
                _logger.LogInformation("Migrating database for context: {ContextType}", contextType.Name);

                try
                {
                    // Apply migrations
                    await dbContext.Database.MigrateAsync(cancellationToken);
                    _logger.LogInformation("Successfully migrated database for context: {ContextType}", contextType.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to migrate database for context: {ContextType}. Error: {Error}",
                        contextType.Name, ex.Message);
                    throw;
                }
            }

            _logger.LogInformation("Database migration completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during database migration: {Error}", ex.Message);
            throw;
        }
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Nothing to do on stop
        return Task.CompletedTask;
    }
}
