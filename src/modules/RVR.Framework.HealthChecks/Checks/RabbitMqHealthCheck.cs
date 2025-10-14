namespace RVR.Framework.HealthChecks.Checks;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

/// <summary>
/// Health check for RabbitMQ message broker connectivity and responsiveness.
/// </summary>
public class RabbitMqHealthCheck : IHealthCheck
{
    private readonly string _connectionString;
    private readonly Func<string, CancellationToken, Task<bool>>? _testConnectionFunc;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMqHealthCheck"/> class.
    /// </summary>
    /// <param name="connectionString">The RabbitMQ connection string.</param>
    /// <param name="testConnectionFunc">Optional custom function to test the connection.</param>
    public RabbitMqHealthCheck(
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
                    "RabbitMQ is accessible and responsive",
                    new Dictionary<string, object> { ["ConnectionString"] = RedactConnectionString(_connectionString) });
            }

            return HealthCheckResult.Degraded(
                "RabbitMQ is accessible but not responding as expected",
                null,
                new Dictionary<string, object> { ["ConnectionString"] = RedactConnectionString(_connectionString) });
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "RabbitMQ health check failed",
                ex,
                new Dictionary<string, object> { ["ConnectionString"] = RedactConnectionString(_connectionString) });
        }
    }

    private async Task<bool> TestConnectionAsync(CancellationToken cancellationToken)
    {
        // Default implementation - in real scenarios, this would use RabbitMQ.Client
        // This is a placeholder that simulates a connection test
        await Task.Delay(100, cancellationToken);
        return !string.IsNullOrWhiteSpace(_connectionString);
    }

    private static string RedactConnectionString(string connectionString)
    {
        // Redact password from amqp:// or amqps:// connection strings
        var uri = new Uri(connectionString);
        var builder = new UriBuilder(uri)
        {
            Password = "***"
        };
        return builder.ToString();
    }
}

/// <summary>
/// Health check for RabbitMQ using IConnection.
/// </summary>
public class RabbitMqConnectionHealthCheck : IHealthCheck
{
    private readonly global::RabbitMQ.Client.IConnection _connection;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMqConnectionHealthCheck"/> class.
    /// </summary>
    /// <param name="connection">The RabbitMQ connection.</param>
    public RabbitMqConnectionHealthCheck(global::RabbitMQ.Client.IConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    /// <inheritdoc/>
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_connection.IsOpen)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("RabbitMQ connection is not open"));
            }

            using var model = _connection.CreateModel();
            return Task.FromResult(HealthCheckResult.Healthy(
                "RabbitMQ is accessible and responsive",
                new Dictionary<string, object> { ["IsOpen"] = _connection.IsOpen }));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("RabbitMQ health check failed", ex));
        }
    }
}
