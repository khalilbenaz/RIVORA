using RVR.Framework.Webhooks.Models;

namespace RVR.Framework.Webhooks;

/// <summary>
/// Provides operations for publishing webhook events and managing subscriptions.
/// </summary>
public interface IWebhookService
{
    /// <summary>
    /// Publishes an event to all matching active subscriptions.
    /// </summary>
    /// <param name="eventType">The event type identifier.</param>
    /// <param name="data">The event payload data.</param>
    /// <param name="tenantId">Optional tenant identifier to scope the publish.</param>
    /// <param name="ct">Cancellation token.</param>
    Task PublishAsync(string eventType, object data, Guid? tenantId = null, CancellationToken ct = default);

    /// <summary>
    /// Creates a new webhook subscription.
    /// </summary>
    /// <param name="subscription">The subscription to create.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created subscription.</returns>
    Task<WebhookSubscription> SubscribeAsync(WebhookSubscription subscription, CancellationToken ct = default);

    /// <summary>
    /// Removes a webhook subscription.
    /// </summary>
    /// <param name="subscriptionId">The subscription identifier to remove.</param>
    /// <param name="ct">Cancellation token.</param>
    Task UnsubscribeAsync(Guid subscriptionId, CancellationToken ct = default);

    /// <summary>
    /// Gets subscriptions matching the specified filters.
    /// </summary>
    /// <param name="eventType">Optional event type filter.</param>
    /// <param name="tenantId">Optional tenant identifier filter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A read-only list of matching subscriptions.</returns>
    Task<IReadOnlyList<WebhookSubscription>> GetSubscriptionsAsync(string? eventType = null, Guid? tenantId = null, CancellationToken ct = default);

    /// <summary>
    /// Gets delivery history for a subscription.
    /// </summary>
    /// <param name="subscriptionId">The subscription identifier.</param>
    /// <param name="take">Maximum number of deliveries to return.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A read-only list of delivery records.</returns>
    Task<IReadOnlyList<WebhookDelivery>> GetDeliveriesAsync(Guid subscriptionId, int take = 20, CancellationToken ct = default);
}
