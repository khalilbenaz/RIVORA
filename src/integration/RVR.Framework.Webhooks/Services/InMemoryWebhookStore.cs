using System.Collections.Concurrent;
using RVR.Framework.Webhooks.Models;

namespace RVR.Framework.Webhooks.Services;

/// <summary>
/// In-memory implementation of <see cref="IWebhookStore"/> using concurrent dictionaries.
/// Suitable for development and testing; not intended for production use.
/// </summary>
public class InMemoryWebhookStore : IWebhookStore
{
    private readonly ConcurrentDictionary<Guid, WebhookSubscription> _subscriptions = new();
    private readonly ConcurrentDictionary<Guid, ConcurrentBag<WebhookDelivery>> _deliveries = new();

    /// <inheritdoc />
    public Task<IReadOnlyList<WebhookSubscription>> GetSubscriptionsByEventAsync(
        string eventType, Guid? tenantId, CancellationToken ct)
    {
        var results = _subscriptions.Values
            .Where(s => s.IsActive && s.EventType == eventType)
            .Where(s => tenantId is null || s.TenantId is null || s.TenantId == tenantId)
            .ToList();

        return Task.FromResult<IReadOnlyList<WebhookSubscription>>(results);
    }

    /// <inheritdoc />
    public Task<WebhookSubscription> AddSubscriptionAsync(WebhookSubscription subscription, CancellationToken ct)
    {
        _subscriptions[subscription.Id] = subscription;
        return Task.FromResult(subscription);
    }

    /// <inheritdoc />
    public Task RemoveSubscriptionAsync(Guid id, CancellationToken ct)
    {
        _subscriptions.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<WebhookSubscription>> GetAllSubscriptionsAsync(
        string? eventType, Guid? tenantId, CancellationToken ct)
    {
        var query = _subscriptions.Values.AsEnumerable();

        if (eventType is not null)
            query = query.Where(s => s.EventType == eventType);

        if (tenantId is not null)
            query = query.Where(s => s.TenantId == tenantId);

        return Task.FromResult<IReadOnlyList<WebhookSubscription>>(query.ToList());
    }

    /// <inheritdoc />
    public Task AddDeliveryAsync(WebhookDelivery delivery, CancellationToken ct)
    {
        var bag = _deliveries.GetOrAdd(delivery.SubscriptionId, _ => []);
        bag.Add(delivery);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<WebhookDelivery>> GetDeliveriesAsync(Guid subscriptionId, int take, CancellationToken ct)
    {
        if (!_deliveries.TryGetValue(subscriptionId, out var bag))
            return Task.FromResult<IReadOnlyList<WebhookDelivery>>([]);

        var results = bag
            .OrderByDescending(d => d.AttemptedAtUtc)
            .Take(take)
            .ToList();

        return Task.FromResult<IReadOnlyList<WebhookDelivery>>(results);
    }
}
