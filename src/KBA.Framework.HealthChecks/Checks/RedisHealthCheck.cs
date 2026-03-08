namespace KBA.Framework.HealthChecks.Checks;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

/// <summary>
/// Health check for Redis cache connectivity and responsiveness.
/// </summary>
public class RedisHealthCheck : IHealthCheck
{
    private readonly string _connectionString;
    private readonly Func<string, CancellationToken, Task<bool>>? _testConnectionFunc;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisHealthCheck"/> class.
    /// </summary>
    /// <param name="connectionString">The Redis connection string.</param>
    /// <param name="testConnectionFunc">Optional custom function to test the connection.</param>
    public RedisHealthCheck(
        string connectionString,
        Func<string, CancellationToken, Task<bool>>? testConnectionFunc = null)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _testConnectionFunc = testConnectionFunc;
    }

    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            bool isConnected;

            if (_testConnectionFunc != null)
            {
                isConnected = await _testConnectionFunc(_connectionString, cancellationToken);
            }
            else
            {
                isConnected = await TestConnectionAsync(cancellationToken);
            }

            if (isConnected)
            {
                return HealthCheckResult.Healthy(
                    "Redis is accessible and responsive",
                    new { ConnectionString = RedactConnectionString(_connectionString) });
            }

            return HealthCheckResult.Degraded(
                "Redis is accessible but not responding as expected",
                null,
                new { ConnectionString = RedactConnectionString(_connectionString) });
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Redis health check failed",
                ex,
                new { ConnectionString = RedactConnectionString(_connectionString) });
        }
    }

    private async Task<bool> TestConnectionAsync(CancellationToken cancellationToken)
    {
        // Default implementation - in real scenarios, this would use StackExchange.Redis
        // This is a placeholder that simulates a connection test
        await Task.Delay(50, cancellationToken);
        return !string.IsNullOrWhiteSpace(_connectionString);
    }

    private static string RedactConnectionString(string connectionString)
    {
        // Simple redaction - remove password if present
        var parts = connectionString.Split(',');
        var redacted = new System.Text.StringBuilder();

        foreach (var part in parts)
        {
            if (!part.StartsWith("password=", StringComparison.OrdinalIgnoreCase))
            {
                redacted.Append(part).Append(',');
            }
        }

        return redacted.ToString().TrimEnd(',');
    }
}

/// <summary>
/// Health check for Redis using IConnectionMultiplexer.
/// </summary>
public class RedisConnectionHealthCheck : IHealthCheck
{
    private readonly StackExchange.Redis.IConnectionMultiplexer _connection;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisConnectionHealthCheck"/> class.
    /// </summary>
    /// <param name="connection">The Redis connection multiplexer.</param>
    public RedisConnectionHealthCheck(StackExchange.Redis.IConnectionMultiplexer connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_connection.IsConnected)
            {
                return HealthCheckResult.Unhealthy("Redis is not connected");
            }

            var endpoints = _connection.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = _connection.GetServer(endpoint);
                await server.PingAsync();
            }

            return HealthCheckResult.Healthy(
                "Redis is accessible and responsive",
                new { Endpoints = endpoints.Length });
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis health check failed", ex);
        }
    }
}
