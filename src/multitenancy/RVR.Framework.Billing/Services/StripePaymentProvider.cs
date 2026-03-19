using RVR.Framework.Billing.Extensions;
using RVR.Framework.Billing.Interfaces;
using RVR.Framework.Core.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace RVR.Framework.Billing.Services;

/// <summary>
/// Stripe implementation of the payment provider abstraction.
/// </summary>
public sealed class StripePaymentProvider : IPaymentProvider
{
    private readonly ILogger<StripePaymentProvider> _logger;
    private readonly BillingOptions _options;
    private readonly StripeClient _stripeClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="StripePaymentProvider"/> class.
    /// </summary>
    public StripePaymentProvider(IOptions<BillingOptions> options, ILogger<StripePaymentProvider> logger)
    {
        _logger = logger;
        _options = options.Value;
        _stripeClient = new StripeClient(_options.StripeApiKey);
    }

    /// <inheritdoc />
    public async Task<string> CreateCheckoutSessionAsync(
        string customerId,
        string priceId,
        string successUrl,
        string cancelUrl,
        int? trialDays = null,
        CancellationToken cancellationToken = default)
    {
        var sessionService = new SessionService(_stripeClient);

        var sessionOptions = new SessionCreateOptions
        {
            Customer = customerId,
            Mode = "subscription",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Price = priceId,
                    Quantity = 1
                }
            ]
        };

        if (trialDays.HasValue && trialDays.Value > 0)
        {
            sessionOptions.SubscriptionData = new SessionSubscriptionDataOptions
            {
                TrialPeriodDays = trialDays.Value
            };
        }

        _logger.LogInformation("Creating Stripe checkout session for customer {CustomerId} with price {PriceId}", LogSanitizer.Sanitize(customerId), LogSanitizer.Sanitize(priceId));

        var session = await sessionService.CreateAsync(sessionOptions, cancellationToken: cancellationToken);

        return session.Url;
    }

    /// <inheritdoc />
    public async Task<string> CreatePortalSessionAsync(
        string customerId,
        string returnUrl,
        CancellationToken cancellationToken = default)
    {
        var portalService = new Stripe.BillingPortal.SessionService(_stripeClient);

        var portalOptions = new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = customerId,
            ReturnUrl = returnUrl
        };

        _logger.LogInformation("Creating Stripe portal session for customer {CustomerId}", LogSanitizer.Sanitize(customerId));

        var session = await portalService.CreateAsync(portalOptions, cancellationToken: cancellationToken);

        return session.Url;
    }

    /// <inheritdoc />
    public async Task CancelSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default)
    {
        var subscriptionService = new SubscriptionService(_stripeClient);

        var updateOptions = new SubscriptionUpdateOptions
        {
            CancelAtPeriodEnd = true
        };

        _logger.LogInformation("Canceling Stripe subscription {SubscriptionId} at period end", LogSanitizer.Sanitize(subscriptionId));

        await subscriptionService.UpdateAsync(subscriptionId, updateOptions, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateSubscriptionAsync(
        string subscriptionId,
        string newPriceId,
        CancellationToken cancellationToken = default)
    {
        var subscriptionService = new SubscriptionService(_stripeClient);

        var subscription = await subscriptionService.GetAsync(subscriptionId, cancellationToken: cancellationToken);

        var updateOptions = new SubscriptionUpdateOptions
        {
            Items =
            [
                new SubscriptionItemOptions
                {
                    Id = subscription.Items.Data[0].Id,
                    Price = newPriceId
                }
            ],
            ProrationBehavior = "create_prorations"
        };

        _logger.LogInformation("Updating Stripe subscription {SubscriptionId} to price {NewPriceId}", LogSanitizer.Sanitize(subscriptionId), LogSanitizer.Sanitize(newPriceId));

        await subscriptionService.UpdateAsync(subscriptionId, updateOptions, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<string> CreateCustomerAsync(
        string email,
        string name,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var customerService = new CustomerService(_stripeClient);

        var customerOptions = new CustomerCreateOptions
        {
            Email = email,
            Name = name,
            Metadata = metadata
        };

        _logger.LogInformation("Creating new Stripe customer");

        var customer = await customerService.CreateAsync(customerOptions, cancellationToken: cancellationToken);

        return customer.Id;
    }

    /// <inheritdoc />
    public async Task<(string Status, DateTime PeriodStart, DateTime PeriodEnd, bool CancelAtPeriodEnd)> GetSubscriptionAsync(
        string subscriptionId,
        CancellationToken cancellationToken = default)
    {
        var subscriptionService = new SubscriptionService(_stripeClient);

        var subscription = await subscriptionService.GetAsync(subscriptionId, cancellationToken: cancellationToken);

        // In Stripe.net v50+, CurrentPeriodStart/End moved to SubscriptionItem
        var firstItem = subscription.Items?.Data?.FirstOrDefault();
        var periodStart = firstItem?.CurrentPeriodStart ?? DateTime.UtcNow;
        var periodEnd = firstItem?.CurrentPeriodEnd ?? DateTime.UtcNow.AddMonths(1);

        return (
            subscription.Status,
            periodStart,
            periodEnd,
            subscription.CancelAtPeriodEnd
        );
    }

}
