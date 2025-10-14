using System.Text.Json;
using RVR.Framework.Webhooks.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RVR.Framework.Webhooks.Services;

/// <summary>
/// Default implementation of <see cref="IWebhookService"/> that publishes events
/// to subscribers by queuing deliveries for background processing.
/// </summary>
public class WebhookService : IWebhookService
{
    private readonly IWebhookStore _store;
    private readonly WebhookDeliveryChannel _channel;
    private readonly WebhookOptions _options;
    private readonly ILogger<WebhookService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Initializes a new instance of <see cref="WebhookService"/>.
    /// </summary>
    public WebhookService(
        IWebhookStore store,
        WebhookDeliveryChannel channel,
        IOptions<WebhookOptions> options,
        ILogger<WebhookService> logger)
    {
        _store = store;
        _channel = channel;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task PublishAsync(string eventType, object data, Guid? tenantId = null, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);

        var subscriptions = await _store.GetSubscriptionsByEventAsync(eventType, tenantId, ct);

        if (subscriptions.Count == 0)
        {
            _logger.LogDebug("No active subscriptions found for event type {EventType}", eventType);
            return;
        }

        var webhookEvent = new WebhookEvent
        {
            Type = eventType,
            TimestampUtc = DateTime.UtcNow,
            Data = data
        };

        var payload = JsonSerializer.Serialize(webhookEvent, JsonOptions);

        _logger.LogInformation(
            "Publishing event {EventType} (id={EventId}) to {Count} subscription(s) via background channel",
            eventType, webhookEvent.Id, subscriptions.Count);

        foreach (var sub in subscriptions)
        {
            var task = new WebhookDeliveryTask
            {
                Subscription = sub,
                WebhookEvent = webhookEvent,
                Payload = payload
            };

            await _channel.Writer.WriteAsync(task, ct);
        }
    }

    /// <inheritdoc />
    public async Task<WebhookSubscription> SubscribeAsync(WebhookSubscription subscription, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(subscription);
        ArgumentException.ThrowIfNullOrWhiteSpace(subscription.EventType);
        ArgumentException.ThrowIfNullOrWhiteSpace(subscription.CallbackUrl);

        // Validate callback URL to prevent SSRF attacks
        CallbackUrlValidator.Validate(subscription.CallbackUrl, _options);

        if (subscription.MaxRetries <= 0)
            subscription.MaxRetries = _options.DefaultMaxRetries;

        var created = await _store.AddSubscriptionAsync(subscription, ct);

        _logger.LogInformation(
            "Created webhook subscription {SubscriptionId} for event {EventType} -> {CallbackUrl}",
            created.Id, created.EventType, created.CallbackUrl);

        return created;
    }

    /// <inheritdoc />
    public async Task UnsubscribeAsync(Guid subscriptionId, CancellationToken ct = default)
    {
        await _store.RemoveSubscriptionAsync(subscriptionId, ct);
        _logger.LogInformation("Removed webhook subscription {SubscriptionId}", subscriptionId);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<WebhookSubscription>> GetSubscriptionsAsync(
        string? eventType = null, Guid? tenantId = null, CancellationToken ct = default)
    {
        return _store.GetAllSubscriptionsAsync(eventType, tenantId, ct);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<WebhookDelivery>> GetDeliveriesAsync(
        Guid subscriptionId, int take = 20, CancellationToken ct = default)
    {
        return _store.GetDeliveriesAsync(subscriptionId, take, ct);
    }
}
