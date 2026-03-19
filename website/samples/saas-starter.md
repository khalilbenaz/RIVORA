# SaaS Starter

Le template **SaaS Starter** genere une application SaaS multi-tenant complete, prete pour la production. C'est le point de depart recommande pour les projets SaaS.

## Creation

```bash
rvr new MonSaaS --template saas-starter
cd MonSaaS
```

## Modules inclus

| Module | Role |
|--------|------|
| Identity.Pro | Authentification JWT, 2FA, OAuth2 |
| Multi-tenancy | Isolation par schema ou base de donnees |
| Billing | Abonnements Stripe, plans, factures |
| Webhooks | Notifications evenementielles |
| Caching | Cache distribue Redis |
| Jobs | Taches planifiees (Hangfire) |
| Export | Export PDF/Excel des donnees |
| Audit | Journal d'audit des actions |

## Entites incluses

Le template genere ces entites metier :

```csharp
// Tenant et gestion utilisateur
public class Tenant : AggregateRoot<Guid>
{
    public string Name { get; private set; }
    public string Subdomain { get; private set; }
    public TenantPlan Plan { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
}

public class TenantUser : Entity<Guid>
{
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public TenantRole Role { get; private set; }
}

// Abonnement et facturation
public class Subscription : AggregateRoot<Guid>
{
    public Guid TenantId { get; private set; }
    public SubscriptionPlan Plan { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public string StripeSubscriptionId { get; private set; }
    public DateTime CurrentPeriodEnd { get; private set; }
}

public class Invoice : Entity<Guid>
{
    public Guid TenantId { get; private set; }
    public Money Amount { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public string StripeInvoiceId { get; private set; }
}
```

## API Endpoints

| Methode | Endpoint | Description |
|---------|----------|-------------|
| POST | `/api/v1/auth/register` | Inscription + creation tenant |
| POST | `/api/v1/auth/login` | Connexion |
| GET | `/api/v1/tenants` | Lister les tenants (admin) |
| PUT | `/api/v1/tenants/:id/plan` | Changer de plan |
| GET | `/api/v1/subscriptions` | Abonnement actuel |
| POST | `/api/v1/subscriptions/checkout` | Lancer le paiement Stripe |
| GET | `/api/v1/invoices` | Historique des factures |
| POST | `/api/v1/webhooks` | Configurer un webhook |

## Configuration

### appsettings.json

```json
{
  "MultiTenancy": {
    "IsEnabled": true,
    "Strategy": "PerSchema",
    "DefaultTenant": "default"
  },
  "Stripe": {
    "SecretKey": "sk_test_...",
    "PublishableKey": "pk_test_...",
    "WebhookSecret": "whsec_...",
    "Plans": {
      "Free": { "PriceId": "price_free", "Limits": { "Users": 3, "Storage": "1GB" } },
      "Pro": { "PriceId": "price_pro", "Limits": { "Users": 25, "Storage": "50GB" } },
      "Enterprise": { "PriceId": "price_enterprise", "Limits": { "Users": -1, "Storage": "unlimited" } }
    }
  }
}
```

## Demarrage

```bash
# 1. Lancer l'infrastructure
docker compose up -d

# 2. Appliquer les migrations
rvr migrate apply

# 3. Seeder les donnees de demo
rvr seed --profile demo

# 4. Lancer l'API
dotnet run --project src/MonSaaS.Api

# 5. (Optionnel) Lancer le frontend
cd src/frontend && npm run dev
```

## Architecture

```
MonSaaS/
  src/
    MonSaaS.Domain/
      Tenants/           # Aggregate Tenant, TenantUser
      Subscriptions/     # Aggregate Subscription
      Invoices/          # Entity Invoice
    MonSaaS.Application/
      Tenants/
        Commands/        # CreateTenant, UpdatePlan
        Queries/         # GetTenantById, GetTenants
      Subscriptions/
        Commands/        # CreateSubscription, CancelSubscription
        Queries/         # GetCurrentSubscription
    MonSaaS.Infrastructure/
      Persistence/       # DbContext, Migrations
      Stripe/            # StripeService, WebhookHandler
    MonSaaS.Api/
      Controllers/       # TenantController, SubscriptionController
      Middleware/         # TenantResolutionMiddleware
```

## Etape suivante

- [Multi-Tenancy](/guide/multi-tenancy) pour comprendre les strategies d'isolation
- [Facturation SaaS](/guide/billing) pour configurer Stripe
- [E-commerce](/samples/ecommerce) pour un autre template
