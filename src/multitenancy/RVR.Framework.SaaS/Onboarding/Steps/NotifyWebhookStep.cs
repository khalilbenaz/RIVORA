using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RVR.Framework.Core.Helpers;

namespace RVR.Framework.SaaS.Onboarding.Steps;

/// <summary>
/// Abstraction for publishing onboarding webhook events.
/// Implement and register this interface in DI to enable webhook delivery;
/// when unregistered the step falls back to logging.
/// </summary>
public interface IOnboardingWebhookPublisher
{
    /// <summary>
    /// Publishes a webhook event for a tenant onboarding action.
    /// </summary>
    Task PublishAsync(string eventType, object payload, Guid? tenantId, CancellationToken ct);
}

/// <summary>
/// Fires a webhook notification for the <c>tenant.provisioned</c> event.
/// Uses <c>IWebhookService</c> (from RVR.Framework.Webhooks) when registered;
/// falls back to logging when the service is unavailable.
/// This step is idempotent — compensation is a no-op since webhook notifications
/// are fire-and-forget events.
/// </summary>
public sealed class NotifyWebhookStep : ITenantOnboardingStep
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotifyWebhookStep> _logger;

    /// <summary>
    /// The event type identifier published to webhook subscribers.
    /// </summary>
    public const string EventType = "tenant.provisioned";

    /// <summary>
    /// Initializes a new instance of <see cref="NotifyWebhookStep"/>.
    /// </summary>
    /// <param name="serviceProvider">Service provider used to resolve optional webhook services at runtime.</param>
    /// <param name="logger">Logger instance.</param>
    public NotifyWebhookStep(
        IServiceProvider serviceProvider,
        ILogger<NotifyWebhookStep> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public string Name => "NotifyWebhook";

    /// <inheritdoc />
    public int Order => 500;

    /// <inheritdoc />
    public async Task<OnboardingStepResult> ExecuteAsync(
        TenantOnboardingContext context,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var payload = new
        {
            TenantId = context.TenantId,
            TenantName = context.TenantName,
            Plan = context.Plan,
            AdminEmail = context.AdminEmail,
            ProvisionedAtUtc = DateTime.UtcNow
        };

        // Resolve IOnboardingWebhookPublisher from DI (Native AOT-safe, no reflection).
        var webhookPublisher = _serviceProvider.GetService<IOnboardingWebhookPublisher>();

        if (webhookPublisher is not null)
        {
            try
            {
                await webhookPublisher.PublishAsync(EventType, payload, context.TenantId, ct);

                _logger.LogInformation(
                    "Webhook '{EventType}' published for tenant {TenantId} via IOnboardingWebhookPublisher",
                    EventType, context.TenantId);

                return new OnboardingStepResult(
                    Name,
                    true,
                    $"Webhook '{EventType}' published successfully");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "IOnboardingWebhookPublisher failed for tenant {TenantId}. Falling back to log-only",
                    context.TenantId);
            }
        }

        // Fallback: log the event.
        _logger.LogInformation(
            "Webhook notification (log-only) for tenant {TenantId}: event='{EventType}', " +
            "name={TenantName}, plan={Plan}",
            context.TenantId, EventType, LogSanitizer.Sanitize(context.TenantName), LogSanitizer.Sanitize(context.Plan));

        return new OnboardingStepResult(
            Name,
            true,
            $"Webhook '{EventType}' logged (no webhook service registered)");
    }

    /// <inheritdoc />
    public Task CompensateAsync(TenantOnboardingContext context, CancellationToken ct = default)
    {
        // Webhook notifications are fire-and-forget; compensation is a no-op.
        _logger.LogInformation(
            "Compensating NotifyWebhook: no action required (webhooks are fire-and-forget) for tenant {TenantId}",
            context.TenantId);

        return Task.CompletedTask;
    }

}
