using RVR.Framework.Webhooks.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RVR.Framework.Webhooks.Services;

/// <summary>
/// Background service that reads webhook delivery tasks from a channel
/// and sends them with retry logic, off the request thread.
/// </summary>
public sealed class WebhookDeliveryBackgroundService : BackgroundService
{
    private readonly WebhookDeliveryChannel _channel;
    private readonly WebhookSender _sender;
    private readonly IWebhookStore _store;
    private readonly WebhookOptions _options;
    private readonly ILogger<WebhookDeliveryBackgroundService> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="WebhookDeliveryBackgroundService"/>.
    /// </summary>
    public WebhookDeliveryBackgroundService(
        WebhookDeliveryChannel channel,
        WebhookSender sender,
        IWebhookStore store,
        IOptions<WebhookOptions> options,
        ILogger<WebhookDeliveryBackgroundService> logger)
    {
        _channel = channel;
        _sender = sender;
        _store = store;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Webhook delivery background service started");

        await foreach (var task in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await DeliverWithRetryAsync(task.Subscription, task.WebhookEvent, task.Payload, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unhandled exception processing webhook delivery for event {EventId} to {CallbackUrl}",
                    task.WebhookEvent.Id, task.Subscription.CallbackUrl);
            }
        }

        _logger.LogInformation("Webhook delivery background service stopped");
    }

    private async Task DeliverWithRetryAsync(
        WebhookSubscription subscription,
        WebhookEvent webhookEvent,
        string payload,
        CancellationToken ct)
    {
        var maxAttempts = subscription.MaxRetries > 0 ? subscription.MaxRetries : _options.DefaultMaxRetries;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var (statusCode, success, errorMessage, duration) =
                await _sender.SendAsync(subscription, webhookEvent, payload, ct);

            var delivery = new WebhookDelivery
            {
                SubscriptionId = subscription.Id,
                EventType = webhookEvent.Type,
                Payload = payload,
                StatusCode = statusCode,
                Success = success,
                ErrorMessage = errorMessage,
                AttemptNumber = attempt,
                AttemptedAtUtc = DateTime.UtcNow,
                Duration = duration
            };

            await _store.AddDeliveryAsync(delivery, ct);

            if (success)
            {
                _logger.LogDebug(
                    "Webhook delivered successfully to {CallbackUrl} on attempt {Attempt}",
                    subscription.CallbackUrl, attempt);
                return;
            }

            if (attempt < maxAttempts)
            {
                // Exponential backoff: 1s, 4s, 16s, ...
                var delaySeconds = (int)Math.Pow(4, attempt - 1);
                _logger.LogWarning(
                    "Webhook delivery to {CallbackUrl} failed (attempt {Attempt}/{MaxAttempts}), retrying in {Delay}s",
                    subscription.CallbackUrl, attempt, maxAttempts, delaySeconds);

                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), ct);
            }
            else
            {
                _logger.LogError(
                    "Webhook delivery to {CallbackUrl} exhausted all {MaxAttempts} attempts for event {EventId}",
                    subscription.CallbackUrl, maxAttempts, webhookEvent.Id);
            }
        }
    }
}
