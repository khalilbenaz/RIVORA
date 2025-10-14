namespace RVR.Framework.Dapr.Services;

/// <summary>
/// Abstraction over the Dapr client for service invocation, pub/sub, state management, and secrets.
/// </summary>
public interface IDaprService
{
    /// <summary>
    /// Invokes a method on a remote Dapr service and deserializes the response.
    /// </summary>
    /// <typeparam name="TResponse">The expected response type.</typeparam>
    /// <param name="appId">The target application ID.</param>
    /// <param name="methodName">The method name to invoke.</param>
    /// <param name="data">The request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized response.</returns>
    Task<TResponse> InvokeMethodAsync<TResponse>(
        string appId,
        string methodName,
        object? data = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes an event to a Dapr pub/sub topic.
    /// </summary>
    /// <param name="pubSubName">The name of the pub/sub component.</param>
    /// <param name="topicName">The topic name.</param>
    /// <param name="data">The event data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishEventAsync(
        string pubSubName,
        string topicName,
        object data,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets state from a Dapr state store.
    /// </summary>
    /// <typeparam name="T">The type of the stored value.</typeparam>
    /// <param name="storeName">The state store name.</param>
    /// <param name="key">The state key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized state value, or default if not found.</returns>
    Task<T?> GetStateAsync<T>(
        string storeName,
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves state to a Dapr state store.
    /// </summary>
    /// <typeparam name="T">The type of the value to store.</typeparam>
    /// <param name="storeName">The state store name.</param>
    /// <param name="key">The state key.</param>
    /// <param name="value">The value to store.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SaveStateAsync<T>(
        string storeName,
        string key,
        T value,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a secret from a Dapr secret store.
    /// </summary>
    /// <param name="storeName">The secret store name.</param>
    /// <param name="key">The secret key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A dictionary of secret key-value pairs.</returns>
    Task<Dictionary<string, string>> GetSecretAsync(
        string storeName,
        string key,
        CancellationToken cancellationToken = default);
}
