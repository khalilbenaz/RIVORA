# Billing Module

**Package**: `RVR.Framework.Billing`

SaaS billing module with plan management, subscriptions, quotas and usage metering.

```csharp
builder.Services.AddRvrBilling(options =>
{
    options.EnableUsageMetering = true;
    options.BillingCycleDay = 1;
});
```

See the [French documentation](/modules/billing) for detailed API reference.
