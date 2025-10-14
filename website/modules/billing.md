# Module Billing

**Package** : `RVR.Framework.Billing`

## Description

Module de facturation SaaS avec gestion des plans, abonnements, quotas et usage metering.

## Enregistrement

```csharp
builder.Services.AddRvrBilling(options =>
{
    options.EnableUsageMetering = true;
    options.BillingCycleDay = 1; // Premier jour du mois
});
```

## Plans et tarification

```csharp
public class Plan
{
    public string Name { get; set; }       // "Starter", "Pro", "Enterprise"
    public decimal MonthlyPrice { get; set; }
    public Dictionary<string, int> Quotas { get; set; } // Feature -> Limit
}
```

### Configuration des plans

```json
{
  "Billing": {
    "Plans": [
      {
        "Name": "Starter",
        "MonthlyPrice": 29,
        "Quotas": { "users": 5, "storage_gb": 10, "api_calls": 10000 }
      },
      {
        "Name": "Pro",
        "MonthlyPrice": 99,
        "Quotas": { "users": 50, "storage_gb": 100, "api_calls": 100000 }
      },
      {
        "Name": "Enterprise",
        "MonthlyPrice": 299,
        "Quotas": { "users": -1, "storage_gb": -1, "api_calls": -1 }
      }
    ]
  }
}
```

## Verification des quotas

```csharp
public class QuotaCheckMiddleware
{
    public async Task InvokeAsync(HttpContext context, IBillingService billing)
    {
        var tenant = context.GetTenantId();
        var canProceed = await billing.CheckQuotaAsync(tenant, "api_calls");

        if (!canProceed)
        {
            context.Response.StatusCode = 429;
            await context.Response.WriteAsJsonAsync(new { error = "Quota exceeded" });
            return;
        }

        await _next(context);
    }
}
```

## Usage Metering

```csharp
await _billing.RecordUsageAsync(tenantId, "api_calls", quantity: 1);
await _billing.RecordUsageAsync(tenantId, "storage_gb", quantity: 0.5m);
```
