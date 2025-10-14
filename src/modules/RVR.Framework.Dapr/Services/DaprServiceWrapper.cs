using Dapr.Client;
using Microsoft.Extensions.Logging;

namespace RVR.Framework.Dapr.Services;

/// <summary>
/// Wraps the Dapr client to provide a simplified interface for common Dapr operations.
/// </summary>
public class DaprServiceWrapper : IDaprService
{
    private readonly DaprClient _daprClient;
    private readonly ILogger<DaprServiceWrapper> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="DaprServiceWrapper"/>.
    /// </summary>
    /// <param name="daprClient">The Dapr client instance.</param>
    /// <param name="logger">The logger.</param>
    public DaprServiceWrapper(DaprClient daprClient, ILogger<DaprServiceWrapper> logger)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<TResponse> InvokeMethodAsync<TResponse>(
        string appId,
        string methodName,
        object? data = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appId);
        ArgumentException.ThrowIfNullOrWhiteSpace(methodName);

        _logger.LogDebug("Invoking method '{MethodName}' on app '{AppId}'", methodName, appId);

        if (data is not null)
        {
            return await _daprClient.InvokeMethodAsync<object, TResponse>(
                appId, methodName, data, cancellationToken);
        }

        return await _daprClient.InvokeMethodAsync<TResponse>(
            appId, methodName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task PublishEventAsync(
        string pubSubName,
        string topicName,
        object data,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pubSubName);
        ArgumentException.ThrowIfNullOrWhiteSpace(topicName);
        ArgumentNullException.ThrowIfNull(data);

        _logger.LogDebug("Publishing event to topic '{TopicName}' on pub/sub '{PubSubName}'", topicName, pubSubName);

        await _daprClient.PublishEventAsync(pubSubName, topicName, data, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<T?> GetStateAsync<T>(
        string storeName,
        string key,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storeName);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        _logger.LogDebug("Getting state for key '{Key}' from store '{StoreName}'", key, storeName);

        return await _daprClient.GetStateAsync<T>(storeName, key, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task SaveStateAsync<T>(
        string storeName,
        string key,
        T value,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storeName);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        _logger.LogDebug("Saving state for key '{Key}' to store '{StoreName}'", key, storeName);

        await _daprClient.SaveStateAsync(storeName, key, value, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, string>> GetSecretAsync(
        string storeName,
        string key,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storeName);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        _logger.LogDebug("Getting secret '{Key}' from store '{StoreName}'", key, storeName);

        return await _daprClient.GetSecretAsync(storeName, key, cancellationToken: cancellationToken);
    }
}
