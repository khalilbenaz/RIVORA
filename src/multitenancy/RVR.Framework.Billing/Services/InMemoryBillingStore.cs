using System.Collections.Concurrent;
using RVR.Framework.Billing.Interfaces;
using RVR.Framework.Billing.Models;

namespace RVR.Framework.Billing.Services;

/// <summary>
/// In-memory implementation of <see cref="IBillingStore"/> for development and testing.
/// Data is not persisted across application restarts.
/// </summary>
public sealed class InMemoryBillingStore : IBillingStore
{
    private readonly ConcurrentDictionary<Guid, SubscriptionPlan> _plans = new();
    private readonly ConcurrentDictionary<string, Subscription> _subscriptionsByTenant = new();
    private readonly ConcurrentDictionary<string, Subscription> _subscriptionsByStripeId = new();
    private readonly ConcurrentDictionary<string, List<Invoice>> _invoicesByTenant = new();
    private readonly ConcurrentDictionary<string, Invoice> _invoicesByStripeId = new();
    private readonly ConcurrentDictionary<string, List<UsageRecord>> _usageByTenant = new();
    private readonly ConcurrentDictionary<string, List<PaymentMethod>> _paymentMethodsByTenant = new();
    private readonly ConcurrentDictionary<string, BillingEvent> _eventsByStripeId = new();
    private readonly ConcurrentDictionary<string, string> _stripeCustomerIds = new();

    // Plans

    /// <inheritdoc />
    public Task<SubscriptionPlan?> GetPlanAsync(Guid planId, CancellationToken cancellationToken = default)
    {
        _plans.TryGetValue(planId, out var plan);
        return Task.FromResult(plan);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<SubscriptionPlan>> GetActivePlansAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<SubscriptionPlan> plans = _plans.Values.Where(p => p.IsActive).ToList();
        return Task.FromResult(plans);
    }

    /// <inheritdoc />
    public Task SavePlanAsync(SubscriptionPlan plan, CancellationToken cancellationToken = default)
    {
        _plans[plan.Id] = plan;
        return Task.CompletedTask;
    }

    // Subscriptions

    /// <inheritdoc />
    public Task<Subscription?> GetSubscriptionByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        _subscriptionsByTenant.TryGetValue(tenantId, out var subscription);
        return Task.FromResult(subscription);
    }

    /// <inheritdoc />
    public Task<Subscription?> GetSubscriptionByStripeIdAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default)
    {
        _subscriptionsByStripeId.TryGetValue(stripeSubscriptionId, out var subscription);
        return Task.FromResult(subscription);
    }

    /// <inheritdoc />
    public Task SaveSubscriptionAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        _subscriptionsByTenant[subscription.TenantId] = subscription;
        if (!string.IsNullOrEmpty(subscription.StripeSubscriptionId))
        {
            _subscriptionsByStripeId[subscription.StripeSubscriptionId] = subscription;
        }
        return Task.CompletedTask;
    }

    // Invoices

    /// <inheritdoc />
    public Task<IReadOnlyList<Invoice>> GetInvoicesByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Invoice> invoices = _invoicesByTenant.TryGetValue(tenantId, out var list)
            ? list.ToList()
            : [];
        return Task.FromResult(invoices);
    }

    /// <inheritdoc />
    public Task<Invoice?> GetInvoiceByStripeIdAsync(string stripeInvoiceId, CancellationToken cancellationToken = default)
    {
        _invoicesByStripeId.TryGetValue(stripeInvoiceId, out var invoice);
        return Task.FromResult(invoice);
    }

    /// <inheritdoc />
    public Task SaveInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        var tenantInvoices = _invoicesByTenant.GetOrAdd(invoice.TenantId, _ => []);
        lock (tenantInvoices)
        {
            var existing = tenantInvoices.FindIndex(i => i.Id == invoice.Id);
            if (existing >= 0)
                tenantInvoices[existing] = invoice;
            else
                tenantInvoices.Add(invoice);
        }

        if (!string.IsNullOrEmpty(invoice.StripeInvoiceId))
        {
            _invoicesByStripeId[invoice.StripeInvoiceId] = invoice;
        }

        return Task.CompletedTask;
    }

    // Usage

    /// <inheritdoc />
    public Task SaveUsageRecordAsync(UsageRecord record, CancellationToken cancellationToken = default)
    {
        var tenantUsage = _usageByTenant.GetOrAdd(record.TenantId, _ => []);
        lock (tenantUsage)
        {
            tenantUsage.Add(record);
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<UsageRecord>> GetUsageRecordsAsync(string tenantId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<UsageRecord> records = _usageByTenant.TryGetValue(tenantId, out var list)
            ? list.Where(r => r.RecordedAt >= from && r.RecordedAt <= to).ToList()
            : [];
        return Task.FromResult(records);
    }

    // Payment Methods

    /// <inheritdoc />
    public Task<IReadOnlyList<PaymentMethod>> GetPaymentMethodsByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PaymentMethod> methods = _paymentMethodsByTenant.TryGetValue(tenantId, out var list)
            ? list.ToList()
            : [];
        return Task.FromResult(methods);
    }

    /// <inheritdoc />
    public Task SavePaymentMethodAsync(PaymentMethod paymentMethod, CancellationToken cancellationToken = default)
    {
        var tenantMethods = _paymentMethodsByTenant.GetOrAdd(paymentMethod.TenantId, _ => []);
        lock (tenantMethods)
        {
            var existing = tenantMethods.FindIndex(m => m.Id == paymentMethod.Id);
            if (existing >= 0)
                tenantMethods[existing] = paymentMethod;
            else
                tenantMethods.Add(paymentMethod);
        }
        return Task.CompletedTask;
    }

    // Events

    /// <inheritdoc />
    public Task SaveBillingEventAsync(BillingEvent billingEvent, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(billingEvent.StripeEventId))
        {
            _eventsByStripeId[billingEvent.StripeEventId] = billingEvent;
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> EventExistsAsync(string stripeEventId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_eventsByStripeId.ContainsKey(stripeEventId));
    }

    // Customer mapping

    /// <inheritdoc />
    public Task<string?> GetStripeCustomerIdAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        _stripeCustomerIds.TryGetValue(tenantId, out var customerId);
        return Task.FromResult(customerId);
    }

    /// <inheritdoc />
    public Task SaveStripeCustomerIdAsync(string tenantId, string stripeCustomerId, CancellationToken cancellationToken = default)
    {
        _stripeCustomerIds[tenantId] = stripeCustomerId;
        return Task.CompletedTask;
    }
}
