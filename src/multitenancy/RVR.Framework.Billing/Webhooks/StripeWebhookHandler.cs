using RVR.Framework.Billing.Extensions;
using RVR.Framework.Billing.Interfaces;
using RVR.Framework.Billing.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace RVR.Framework.Billing.Webhooks;

/// <summary>
/// Handles incoming Stripe webhook events with signature verification.
/// </summary>
public sealed class StripeWebhookHandler
{
    private readonly IBillingStore _billingStore;
    private readonly ILogger<StripeWebhookHandler> _logger;
    private readonly BillingOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="StripeWebhookHandler"/> class.
    /// </summary>
    public StripeWebhookHandler(
        IBillingStore billingStore,
        IOptions<BillingOptions> options,
        ILogger<StripeWebhookHandler> logger)
    {
        _billingStore = billingStore;
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    /// Processes a Stripe webhook request by verifying the signature and handling the event.
    /// </summary>
    /// <param name="requestBody">The raw request body.</param>
    /// <param name="stripeSignature">The Stripe-Signature header value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the event was processed successfully; false otherwise.</returns>
    public async Task<bool> HandleWebhookAsync(string requestBody, string stripeSignature, CancellationToken cancellationToken = default)
    {
        Event stripeEvent;

        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                requestBody,
                stripeSignature,
                _options.StripeWebhookSecret);
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Failed to verify Stripe webhook signature");
            return false;
        }

        // Idempotency check
        if (await _billingStore.EventExistsAsync(stripeEvent.Id, cancellationToken))
        {
            _logger.LogInformation("Stripe event {EventId} already processed, skipping", stripeEvent.Id);
            return true;
        }

        _logger.LogInformation("Processing Stripe event {EventId} of type {EventType}", stripeEvent.Id, stripeEvent.Type);

        var handled = stripeEvent.Type switch
        {
            EventTypes.InvoicePaid => await HandleInvoicePaidAsync(stripeEvent, cancellationToken),
            EventTypes.InvoicePaymentFailed => await HandleInvoicePaymentFailedAsync(stripeEvent, cancellationToken),
            EventTypes.CustomerSubscriptionUpdated => await HandleSubscriptionUpdatedAsync(stripeEvent, cancellationToken),
            EventTypes.CustomerSubscriptionDeleted => await HandleSubscriptionDeletedAsync(stripeEvent, cancellationToken),
            EventTypes.CheckoutSessionCompleted => await HandleCheckoutSessionCompletedAsync(stripeEvent, cancellationToken),
            _ => HandleUnknownEvent(stripeEvent)
        };

        // Save the event for audit/idempotency
        var billingEvent = new BillingEvent
        {
            EventType = stripeEvent.Type,
            StripeEventId = stripeEvent.Id,
            Payload = requestBody,
            ProcessedAt = DateTime.UtcNow
        };

        await _billingStore.SaveBillingEventAsync(billingEvent, cancellationToken);

        return handled;
    }

    private async Task<bool> HandleInvoicePaidAsync(Event stripeEvent, CancellationToken cancellationToken)
    {
        if (stripeEvent.Data.Object is not Stripe.Invoice stripeInvoice)
        {
            _logger.LogWarning("Could not deserialize invoice from event {EventId}", stripeEvent.Id);
            return false;
        }

        var invoice = await _billingStore.GetInvoiceByStripeIdAsync(stripeInvoice.Id, cancellationToken);

        if (invoice is null)
        {
            invoice = new Models.Invoice
            {
                StripeInvoiceId = stripeInvoice.Id,
                Amount = stripeInvoice.AmountPaid,
                Currency = stripeInvoice.Currency,
                TenantId = stripeInvoice.Metadata?.GetValueOrDefault("tenant_id") ?? string.Empty
            };
        }

        invoice.Status = InvoiceStatus.Paid;
        invoice.PaidAt = DateTime.UtcNow;

        await _billingStore.SaveInvoiceAsync(invoice, cancellationToken);

        _logger.LogInformation("Invoice {InvoiceId} marked as paid", stripeInvoice.Id);
        return true;
    }

    private async Task<bool> HandleInvoicePaymentFailedAsync(Event stripeEvent, CancellationToken cancellationToken)
    {
        if (stripeEvent.Data.Object is not Stripe.Invoice stripeInvoice)
        {
            _logger.LogWarning("Could not deserialize invoice from event {EventId}", stripeEvent.Id);
            return false;
        }

        var invoice = await _billingStore.GetInvoiceByStripeIdAsync(stripeInvoice.Id, cancellationToken);

        if (invoice is null)
        {
            invoice = new Models.Invoice
            {
                StripeInvoiceId = stripeInvoice.Id,
                Amount = stripeInvoice.AmountDue,
                Currency = stripeInvoice.Currency,
                TenantId = stripeInvoice.Metadata?.GetValueOrDefault("tenant_id") ?? string.Empty
            };
        }

        invoice.Status = InvoiceStatus.Open;

        await _billingStore.SaveInvoiceAsync(invoice, cancellationToken);

        // Mark subscription as past due if applicable
        var invoiceSubscriptionId = stripeInvoice.Parent?.SubscriptionDetails?.SubscriptionId;
        if (!string.IsNullOrEmpty(invoiceSubscriptionId))
        {
            var subscription = await _billingStore.GetSubscriptionByStripeIdAsync(invoiceSubscriptionId, cancellationToken);
            if (subscription is not null)
            {
                subscription.Status = SubscriptionStatus.PastDue;
                await _billingStore.SaveSubscriptionAsync(subscription, cancellationToken);
            }
        }

        _logger.LogWarning("Invoice {InvoiceId} payment failed", stripeInvoice.Id);
        return true;
    }

    private async Task<bool> HandleSubscriptionUpdatedAsync(Event stripeEvent, CancellationToken cancellationToken)
    {
        if (stripeEvent.Data.Object is not Stripe.Subscription stripeSub)
        {
            _logger.LogWarning("Could not deserialize subscription from event {EventId}", stripeEvent.Id);
            return false;
        }

        var subscription = await _billingStore.GetSubscriptionByStripeIdAsync(stripeSub.Id, cancellationToken);

        if (subscription is null)
        {
            _logger.LogWarning("Subscription {StripeSubscriptionId} not found in store", stripeSub.Id);
            return false;
        }

        // In Stripe.net v50+, CurrentPeriodStart/End moved to SubscriptionItem
        var firstItem = stripeSub.Items?.Data?.FirstOrDefault();

        subscription.Status = MapSubscriptionStatus(stripeSub.Status);
        subscription.CurrentPeriodStart = firstItem?.CurrentPeriodStart ?? DateTime.UtcNow;
        subscription.CurrentPeriodEnd = firstItem?.CurrentPeriodEnd ?? DateTime.UtcNow.AddMonths(1);
        subscription.CancelAtPeriodEnd = stripeSub.CancelAtPeriodEnd;

        await _billingStore.SaveSubscriptionAsync(subscription, cancellationToken);

        _logger.LogInformation("Subscription {StripeSubscriptionId} updated to status {Status}", stripeSub.Id, subscription.Status);
        return true;
    }

    private async Task<bool> HandleSubscriptionDeletedAsync(Event stripeEvent, CancellationToken cancellationToken)
    {
        if (stripeEvent.Data.Object is not Stripe.Subscription stripeSub)
        {
            _logger.LogWarning("Could not deserialize subscription from event {EventId}", stripeEvent.Id);
            return false;
        }

        var subscription = await _billingStore.GetSubscriptionByStripeIdAsync(stripeSub.Id, cancellationToken);

        if (subscription is null)
        {
            _logger.LogWarning("Subscription {StripeSubscriptionId} not found in store", stripeSub.Id);
            return false;
        }

        subscription.Status = SubscriptionStatus.Canceled;

        await _billingStore.SaveSubscriptionAsync(subscription, cancellationToken);

        _logger.LogInformation("Subscription {StripeSubscriptionId} deleted/canceled", stripeSub.Id);
        return true;
    }

    private async Task<bool> HandleCheckoutSessionCompletedAsync(Event stripeEvent, CancellationToken cancellationToken)
    {
        if (stripeEvent.Data.Object is not Stripe.Checkout.Session session)
        {
            _logger.LogWarning("Could not deserialize checkout session from event {EventId}", stripeEvent.Id);
            return false;
        }

        if (string.IsNullOrEmpty(session.SubscriptionId))
        {
            _logger.LogInformation("Checkout session {SessionId} completed but has no subscription", session.Id);
            return true;
        }

        var tenantId = session.Metadata?.GetValueOrDefault("tenant_id") ?? string.Empty;

        var subscription = new Models.Subscription
        {
            TenantId = tenantId,
            StripeSubscriptionId = session.SubscriptionId,
            Status = SubscriptionStatus.Active,
            CurrentPeriodStart = DateTime.UtcNow,
            CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1)
        };

        await _billingStore.SaveSubscriptionAsync(subscription, cancellationToken);

        _logger.LogInformation("Checkout session {SessionId} completed, subscription {SubscriptionId} created for tenant {TenantId}",
            session.Id, session.SubscriptionId, tenantId);
        return true;
    }

    private bool HandleUnknownEvent(Event stripeEvent)
    {
        _logger.LogDebug("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
        return true;
    }

    private static SubscriptionStatus MapSubscriptionStatus(string stripeStatus) => stripeStatus switch
    {
        "trialing" => SubscriptionStatus.Trialing,
        "active" => SubscriptionStatus.Active,
        "past_due" => SubscriptionStatus.PastDue,
        "canceled" => SubscriptionStatus.Canceled,
        "unpaid" => SubscriptionStatus.Suspended,
        _ => SubscriptionStatus.Active
    };
}
