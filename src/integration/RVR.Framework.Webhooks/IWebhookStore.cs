using RVR.Framework.Webhooks.Models;

namespace RVR.Framework.Webhooks;

/// <summary>
/// Persistence abstraction for webhook subscriptions and delivery records.
/// </summary>
public interface IWebhookStore
{
    /// <summary>
    /// Gets all active subscriptions for a given event type, optionally scoped to a tenant.
    /// </summary>
    Task<IReadOnlyList<WebhookSubscription>> GetSubscriptionsByEventAsync(string eventType, Guid? tenantId, CancellationToken ct);

    /// <summary>
    /// Adds a new subscription.
    /// </summary>
    Task<WebhookSubscription> AddSubscriptionAsync(WebhookSubscription subscription, CancellationToken ct);

    /// <summary>
    /// Removes a subscription by its identifier.
    /// </summary>
    Task RemoveSubscriptionAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Gets all subscriptions matching optional filters.
    /// </summary>
    Task<IReadOnlyList<WebhookSubscription>> GetAllSubscriptionsAsync(string? eventType, Guid? tenantId, CancellationToken ct);

    /// <summary>
    /// Records a delivery attempt.
    /// </summary>
    Task AddDeliveryAsync(WebhookDelivery delivery, CancellationToken ct);

    /// <summary>
    /// Gets delivery history for a subscription.
    /// </summary>
    Task<IReadOnlyList<WebhookDelivery>> GetDeliveriesAsync(Guid subscriptionId, int take, CancellationToken ct);
}
