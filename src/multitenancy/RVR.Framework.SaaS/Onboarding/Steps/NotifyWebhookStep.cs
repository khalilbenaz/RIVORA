using Microsoft.Extensions.Logging;

namespace RVR.Framework.SaaS.Onboarding.Steps;

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

        // Try to resolve IWebhookService dynamically to avoid a hard dependency on RVR.Framework.Webhooks.
        var webhookService = ResolveWebhookService();

        if (webhookService is not null)
        {
            try
            {
                await PublishViaWebhookServiceAsync(webhookService, payload, context.TenantId, ct);

                _logger.LogInformation(
                    "Webhook '{EventType}' published for tenant {TenantId} via IWebhookService",
                    EventType, context.TenantId);

                return new OnboardingStepResult(
                    Name,
                    true,
                    $"Webhook '{EventType}' published successfully");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "IWebhookService failed for tenant {TenantId}. Falling back to log-only",
                    context.TenantId);
            }
        }

        // Fallback: log the event.
        _logger.LogInformation(
            "Webhook notification (log-only) for tenant {TenantId}: event='{EventType}', " +
            "name={TenantName}, plan={Plan}",
            context.TenantId, EventType, context.TenantName, context.Plan);

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

    /// <summary>
    /// Attempts to resolve an IWebhookService from the DI container by interface name.
    /// Uses reflection to avoid a compile-time dependency on RVR.Framework.Webhooks.
    /// </summary>
    private object? ResolveWebhookService()
    {
        var webhookServiceType = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a =>
            {
                try { return a.GetTypes(); }
                catch { return []; }
            })
            .FirstOrDefault(t => t.IsInterface && t.FullName == "RVR.Framework.Webhooks.IWebhookService");

        if (webhookServiceType is not null)
        {
            return _serviceProvider.GetService(webhookServiceType);
        }

        return null;
    }

    /// <summary>
    /// Publishes the webhook event via the resolved IWebhookService using reflection.
    /// </summary>
    private static async Task PublishViaWebhookServiceAsync(
        object webhookService,
        object payload,
        Guid tenantId,
        CancellationToken ct)
    {
        var method = webhookService.GetType().GetMethod("PublishAsync");
        if (method is null)
            throw new InvalidOperationException("IWebhookService does not expose a PublishAsync method.");

        var task = method.Invoke(webhookService, [EventType, payload, (Guid?)tenantId, ct]) as Task;
        if (task is not null)
            await task;
    }
}
