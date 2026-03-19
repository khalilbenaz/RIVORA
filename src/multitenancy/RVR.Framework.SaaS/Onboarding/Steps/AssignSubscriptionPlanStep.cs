using Microsoft.Extensions.Logging;
using RVR.Framework.Core.Helpers;
using RVR.Framework.SaaS.Interfaces;

namespace RVR.Framework.SaaS.Onboarding.Steps;

/// <summary>
/// Assigns the requested subscription plan to the tenant.
/// Uses <see cref="ISubscriptionService"/> when registered; otherwise performs
/// an in-memory assignment and logs the outcome.
/// </summary>
public sealed class AssignSubscriptionPlanStep : ITenantOnboardingStep
{
    private readonly ISubscriptionService? _subscriptionService;
    private readonly ILogger<AssignSubscriptionPlanStep> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="AssignSubscriptionPlanStep"/>.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="subscriptionService">Optional subscription service. When null the step logs the assignment only.</param>
    public AssignSubscriptionPlanStep(
        ILogger<AssignSubscriptionPlanStep> logger,
        ISubscriptionService? subscriptionService = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _subscriptionService = subscriptionService;
    }

    /// <inheritdoc />
    public string Name => "AssignSubscriptionPlan";

    /// <inheritdoc />
    public int Order => 300;

    /// <inheritdoc />
    public async Task<OnboardingStepResult> ExecuteAsync(
        TenantOnboardingContext context,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var plan = context.Plan;
        if (string.IsNullOrWhiteSpace(plan))
        {
            plan = "free";
        }

        _logger.LogInformation(
            "Assigning subscription plan '{Plan}' to tenant {TenantId}",
            LogSanitizer.Sanitize(plan), context.TenantId);

        if (_subscriptionService is not null)
        {
            try
            {
                var sessionId = await _subscriptionService.CreateCheckoutSessionAsync(
                    context.TenantId.ToString(), plan);

                context.Properties["CheckoutSessionId"] = sessionId;

                _logger.LogInformation(
                    "Checkout session {SessionId} created for tenant {TenantId} on plan '{Plan}'",
                    LogSanitizer.Sanitize(sessionId), context.TenantId, LogSanitizer.Sanitize(plan));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to create checkout session for tenant {TenantId} on plan '{Plan}'",
                    context.TenantId, LogSanitizer.Sanitize(plan));

                return new OnboardingStepResult(Name, false, $"Failed to assign plan: {ex.Message}");
            }
        }
        else
        {
            _logger.LogInformation(
                "No ISubscriptionService registered. Plan '{Plan}' assigned in-memory for tenant {TenantId}",
                LogSanitizer.Sanitize(plan), context.TenantId);
        }

        context.Properties["AssignedPlan"] = plan;

        return new OnboardingStepResult(
            Name,
            true,
            $"Plan '{plan}' assigned successfully",
            new Dictionary<string, object> { ["Plan"] = plan });
    }

    /// <inheritdoc />
    public Task CompensateAsync(TenantOnboardingContext context, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        // In production, cancel the checkout session or revert the subscription.
        if (context.Properties.TryGetValue("AssignedPlan", out var plan))
        {
            _logger.LogInformation(
                "Compensating AssignSubscriptionPlan: revoking plan '{Plan}' for tenant {TenantId}",
                plan, context.TenantId);

            context.Properties.Remove("AssignedPlan");
            context.Properties.Remove("CheckoutSessionId");
        }
        else
        {
            _logger.LogWarning(
                "Compensating AssignSubscriptionPlan: no AssignedPlan found in context for tenant {TenantId}",
                context.TenantId);
        }

        return Task.CompletedTask;
    }
}
