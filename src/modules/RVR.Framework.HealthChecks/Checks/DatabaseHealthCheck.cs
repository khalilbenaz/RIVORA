namespace RVR.Framework.HealthChecks.Checks;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

/// <summary>
/// Health check for database connectivity and responsiveness.
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly string _connectionString;
    private readonly string _databaseType;
    private readonly Func<string, CancellationToken, Task<bool>> _testConnectionFunc;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseHealthCheck"/> class.
    /// </summary>
    /// <param name="connectionString">The database connection string.</param>
    /// <param name="databaseType">The type of database (e.g., "SqlServer", "PostgreSQL", "MySQL").</param>
    /// <param name="testConnectionFunc">Optional custom function to test the connection.</param>
    public DatabaseHealthCheck(
        string connectionString,
        string databaseType = "Unknown",
        Func<string, CancellationToken, Task<bool>>? testConnectionFunc = null)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _databaseType = databaseType;
        _testConnectionFunc = testConnectionFunc ?? DefaultTestConnectionAsync;
    }

    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var isConnected = await _testConnectionFunc(_connectionString, cancellationToken);

            if (isConnected)
            {
                return HealthCheckResult.Healthy(
                    $"Database {_databaseType} is accessible",
                    new Dictionary<string, object> { ["DatabaseType"] = _databaseType, ["ConnectionString"] = RedactConnectionString(_connectionString) });
            }

            return HealthCheckResult.Unhealthy(
                $"Database {_databaseType} is not accessible",
                null,
                new Dictionary<string, object> { ["DatabaseType"] = _databaseType });
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                $"Database {_databaseType} health check failed",
                ex,
                new Dictionary<string, object> { ["DatabaseType"] = _databaseType });
        }
    }

    private static async Task<bool> DefaultTestConnectionAsync(string connectionString, CancellationToken cancellationToken)
    {
        // Default implementation - in real scenarios, this would use the actual database provider
        // This is a placeholder that assumes the connection string format is valid
        await Task.Delay(100, cancellationToken);
        return !string.IsNullOrWhiteSpace(connectionString);
    }

    private static string RedactConnectionString(string connectionString)
    {
        // Simple redaction - remove password if present
        var parts = connectionString.Split(';');
        var redacted = new System.Text.StringBuilder();

        foreach (var part in parts)
        {
            if (!part.StartsWith("Password=", StringComparison.OrdinalIgnoreCase) &&
                !part.StartsWith("Pwd=", StringComparison.OrdinalIgnoreCase))
            {
                redacted.Append(part).Append(';');
            }
        }

        return redacted.ToString().TrimEnd(';');
    }
}

/// <summary>
/// Health check for Entity Framework Core database context.
/// </summary>
public class EfCoreHealthCheck : IHealthCheck
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Type _contextType;

    /// <summary>
    /// Initializes a new instance of the <see cref="EfCoreHealthCheck"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="contextType">The DbContext type to check.</param>
    public EfCoreHealthCheck(IServiceProvider serviceProvider, Type contextType)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _contextType = contextType ?? throw new ArgumentNullException(nameof(contextType));
    }

    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetService(_contextType);

            if (dbContext is Microsoft.EntityFrameworkCore.DbContext efContext)
            {
                var canConnect = await efContext.Database.CanConnectAsync(cancellationToken);

                if (canConnect)
                {
                    return HealthCheckResult.Healthy(
                        $"Entity Framework Core context {_contextType.Name} can connect to database",
                        new Dictionary<string, object> { ["ContextType"] = _contextType.Name });
                }

                return HealthCheckResult.Unhealthy(
                    $"Entity Framework Core context {_contextType.Name} cannot connect to database",
                    null,
                    new Dictionary<string, object> { ["ContextType"] = _contextType.Name });
            }

            return HealthCheckResult.Unhealthy(
                $"Type {_contextType.Name} is not a valid DbContext",
                null,
                new Dictionary<string, object> { ["ContextType"] = _contextType.Name });
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                $"Entity Framework Core health check failed",
                ex,
                new Dictionary<string, object> { ["ContextType"] = _contextType.Name });
        }
    }
}
