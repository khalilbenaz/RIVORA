# SaaS Billing Guide

This guide covers setting up Stripe-based billing for your RIVORA Framework SaaS application, including subscriptions, usage metering, and webhook handling.

## Prerequisites

- A Stripe account (test mode is fine for development)
- Stripe API keys from the Stripe Dashboard
- RIVORA Framework with the `RVR.Framework.Billing` module

## Step 1: Configure Stripe

Add your Stripe keys to `appsettings.json`:

```json
{
  "Billing": {
    "Stripe": {
      "SecretKey": "sk_test_...",
      "PublishableKey": "pk_test_...",
      "WebhookSecret": "whsec_...",
      "SuccessUrl": "https://app.example.com/billing/success",
      "CancelUrl": "https://app.example.com/billing/cancel"
    }
  }
}
```

::: warning
Never commit real Stripe keys to source control. Use user secrets or environment variables in production.
:::

## Step 2: Register Billing Services

```csharp
// In Program.cs
builder.Services.AddRvrBilling(options =>
{
    options.StripeSecretKey = builder.Configuration["Billing:Stripe:SecretKey"]!;
    options.StripeWebhookSecret = builder.Configuration["Billing:Stripe:WebhookSecret"]!;
    options.EnableUsageMetering = true;
});

// Map billing endpoints (Minimal API)
app.MapBillingEndpoints();
```

## Step 3: Define Subscription Plans

Create plans that map to Stripe products/prices:

```csharp
public class PlanSeeder
{
    public async Task SeedPlansAsync(IBillingService billingService, CancellationToken ct)
    {
        var plans = new[]
        {
            new SubscriptionPlan
            {
                Id = Guid.Parse("..."),
                Name = "Starter",
                StripePriceId = "price_starter_monthly",
                MonthlyPrice = 29.00m,
                Features = new[] { "5 users", "1 GB storage", "Email support" }
            },
            new SubscriptionPlan
            {
                Id = Guid.Parse("..."),
                Name = "Professional",
                StripePriceId = "price_pro_monthly",
                MonthlyPrice = 99.00m,
                Features = new[] { "25 users", "10 GB storage", "Priority support", "API access" }
            },
            new SubscriptionPlan
            {
                Id = Guid.Parse("..."),
                Name = "Enterprise",
                StripePriceId = "price_enterprise_monthly",
                MonthlyPrice = 299.00m,
                Features = new[] { "Unlimited users", "100 GB storage", "24/7 support", "SLA", "Custom integrations" }
            }
        };

        foreach (var plan in plans)
        {
            await billingService.UpsertPlanAsync(plan, ct);
        }
    }
}
```

## Step 4: Create Checkout Sessions

When a tenant wants to subscribe, create a Stripe Checkout session:

```csharp
// POST /api/billing/checkout
// Request:
{
    "tenantId": "tenant-abc",
    "planId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "successUrl": "https://app.example.com/billing/success?session_id={CHECKOUT_SESSION_ID}",
    "cancelUrl": "https://app.example.com/billing/cancel"
}

// Response:
{
    "url": "https://checkout.stripe.com/c/pay/cs_test_..."
}
```

Redirect the user to the returned URL to complete payment.

## Step 5: Customer Portal

Let tenants manage their subscription (upgrade, downgrade, cancel, update payment method):

```csharp
// POST /api/billing/portal
// Request:
{
    "tenantId": "tenant-abc",
    "returnUrl": "https://app.example.com/billing"
}

// Response:
{
    "url": "https://billing.stripe.com/p/session/..."
}
```

## Step 6: Usage Metering

Track consumption-based metrics for metered billing:

```csharp
public class ApiUsageMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IBillingService _billingService;

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        // Record API call usage for the current tenant
        var tenantId = context.Items["__TenantInfo__"] as TenantInfo;
        if (tenantId != null)
        {
            await _billingService.RecordUsageAsync(
                tenantId.Id,
                metricName: "api_calls",
                quantity: 1);
        }
    }
}
```

Report usage via the API:

```csharp
// POST /api/billing/usage
{
    "tenantId": "tenant-abc",
    "metricName": "api_calls",
    "quantity": 150
}
```

## Step 7: Handle Stripe Webhooks

The billing module includes a webhook handler at `/api/billing/webhooks/stripe`. Configure this URL in your Stripe Dashboard under **Developers > Webhooks**.

### Events handled

| Stripe Event | Action |
|-------------|--------|
| `checkout.session.completed` | Activate subscription for tenant |
| `invoice.paid` | Record payment, extend subscription |
| `invoice.payment_failed` | Notify tenant, flag account |
| `customer.subscription.updated` | Update plan details |
| `customer.subscription.deleted` | Deactivate subscription |

### Local webhook testing

Use the Stripe CLI to forward webhooks locally:

```bash
stripe listen --forward-to http://localhost:5220/api/billing/webhooks/stripe
```

The CLI will output a webhook signing secret (`whsec_...`). Use it in your `appsettings.Development.json`.

## Step 8: Query Subscription Status

```csharp
// GET /api/billing/subscription?tenantId=tenant-abc
{
    "id": "sub_1234",
    "tenantId": "tenant-abc",
    "planId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "planName": "Professional",
    "status": "active",
    "currentPeriodStart": "2026-03-01T00:00:00Z",
    "currentPeriodEnd": "2026-04-01T00:00:00Z"
}
```

### Check subscription in code

```csharp
public class FeatureGateService
{
    private readonly IBillingService _billing;

    public async Task<bool> HasFeatureAsync(string tenantId, string feature, CancellationToken ct)
    {
        var subscription = await _billing.GetSubscriptionAsync(tenantId, ct);

        if (subscription == null || subscription.Status != "active")
            return false;

        var plan = await _billing.GetPlanAsync(subscription.PlanId, ct);
        return plan.Features.Contains(feature);
    }
}
```

## Step 9: Invoices

```csharp
// GET /api/billing/invoices?tenantId=tenant-abc
[
    {
        "id": "inv_1234",
        "amount": 9900,
        "currency": "usd",
        "status": "paid",
        "pdfUrl": "https://pay.stripe.com/invoice/...",
        "createdAt": "2026-03-01T00:00:00Z"
    }
]
```

## Architecture Overview

```
[Tenant UI] --> POST /api/billing/checkout --> [Stripe Checkout]
                                                     |
                                              (payment completed)
                                                     |
[Stripe] --> POST /api/billing/webhooks/stripe --> [WebhookHandler]
                                                     |
                                              [Update Subscription]
                                                     |
                                              [Notify Tenant via SignalR]
```
