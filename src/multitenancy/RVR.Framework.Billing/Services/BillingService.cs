using RVR.Framework.Billing.Extensions;
using RVR.Framework.Billing.Interfaces;
using RVR.Framework.Billing.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RVR.Framework.Billing.Services;

/// <summary>
/// Default implementation of <see cref="IBillingService"/> coordinating between
/// the payment provider and the billing store.
/// </summary>
public sealed class BillingService : IBillingService
{
    private readonly IPaymentProvider _paymentProvider;
    private readonly IBillingStore _billingStore;
    private readonly ILogger<BillingService> _logger;
    private readonly BillingOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="BillingService"/> class.
    /// </summary>
    public BillingService(
        IPaymentProvider paymentProvider,
        IBillingStore billingStore,
        IOptions<BillingOptions> options,
        ILogger<BillingService> logger)
    {
        _paymentProvider = paymentProvider;
        _billingStore = billingStore;
        _logger = logger;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<string> CreateCheckoutSessionAsync(
        string tenantId,
        Guid planId,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default)
    {
        var plan = await _billingStore.GetPlanAsync(planId, cancellationToken)
            ?? throw new InvalidOperationException($"Plan {planId} not found.");

        var customerId = await GetOrCreateStripeCustomerAsync(tenantId, cancellationToken);

        // Use plan name as Stripe price ID placeholder; in production, plans would store Stripe price IDs.
        var priceId = plan.Name;

        var trialDays = plan.TrialDays > 0 ? plan.TrialDays : _options.TrialDays;

        _logger.LogInformation("Creating checkout session for tenant {TenantId}, plan {PlanId}", tenantId, planId);

        return await _paymentProvider.CreateCheckoutSessionAsync(
            customerId, priceId, successUrl, cancelUrl, trialDays > 0 ? trialDays : null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<string> ManageSubscriptionAsync(
        string tenantId,
        string returnUrl,
        CancellationToken cancellationToken = default)
    {
        var customerId = await _billingStore.GetStripeCustomerIdAsync(tenantId, cancellationToken)
            ?? throw new InvalidOperationException($"No Stripe customer found for tenant {tenantId}.");

        _logger.LogInformation("Creating portal session for tenant {TenantId}", tenantId);

        return await _paymentProvider.CreatePortalSessionAsync(customerId, returnUrl, cancellationToken);
    }

    /// <inheritdoc />
    public async Task CancelSubscriptionAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var subscription = await _billingStore.GetSubscriptionByTenantAsync(tenantId, cancellationToken)
            ?? throw new InvalidOperationException($"No subscription found for tenant {tenantId}.");

        _logger.LogInformation("Canceling subscription for tenant {TenantId}", tenantId);

        await _paymentProvider.CancelSubscriptionAsync(subscription.StripeSubscriptionId, cancellationToken);

        subscription.CancelAtPeriodEnd = true;
        await _billingStore.SaveSubscriptionAsync(subscription, cancellationToken);
    }

    /// <inheritdoc />
    public async Task ChangeSubscriptionPlanAsync(
        string tenantId,
        Guid newPlanId,
        CancellationToken cancellationToken = default)
    {
        var subscription = await _billingStore.GetSubscriptionByTenantAsync(tenantId, cancellationToken)
            ?? throw new InvalidOperationException($"No subscription found for tenant {tenantId}.");

        var newPlan = await _billingStore.GetPlanAsync(newPlanId, cancellationToken)
            ?? throw new InvalidOperationException($"Plan {newPlanId} not found.");

        _logger.LogInformation("Changing subscription for tenant {TenantId} to plan {PlanId}", tenantId, newPlanId);

        await _paymentProvider.UpdateSubscriptionAsync(subscription.StripeSubscriptionId, newPlan.Name, cancellationToken);

        subscription.PlanId = newPlanId;
        await _billingStore.SaveSubscriptionAsync(subscription, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Subscription?> GetSubscriptionAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        return await _billingStore.GetSubscriptionByTenantAsync(tenantId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Invoice>> GetInvoicesAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        return await _billingStore.GetInvoicesByTenantAsync(tenantId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RecordUsageAsync(
        string tenantId,
        string metricName,
        long quantity,
        CancellationToken cancellationToken = default)
    {
        var record = new UsageRecord
        {
            TenantId = tenantId,
            MetricName = metricName,
            Quantity = quantity,
            RecordedAt = DateTime.UtcNow
        };

        _logger.LogDebug("Recording usage for tenant {TenantId}: {MetricName} = {Quantity}", tenantId, metricName, quantity);

        await _billingStore.SaveUsageRecordAsync(record, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, long>> GetUsageSummaryAsync(
        string tenantId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        var records = await _billingStore.GetUsageRecordsAsync(tenantId, from, to, cancellationToken);

        return records
            .GroupBy(r => r.MetricName)
            .ToDictionary(g => g.Key, g => g.Sum(r => r.Quantity));
    }

    private async Task<string> GetOrCreateStripeCustomerAsync(string tenantId, CancellationToken cancellationToken)
    {
        var existingCustomerId = await _billingStore.GetStripeCustomerIdAsync(tenantId, cancellationToken);
        if (!string.IsNullOrEmpty(existingCustomerId))
        {
            return existingCustomerId;
        }

        var customerId = await _paymentProvider.CreateCustomerAsync(
            email: $"{tenantId}@billing.local",
            name: tenantId,
            metadata: new Dictionary<string, string> { ["tenant_id"] = tenantId },
            cancellationToken: cancellationToken);

        await _billingStore.SaveStripeCustomerIdAsync(tenantId, customerId, cancellationToken);

        return customerId;
    }
}
