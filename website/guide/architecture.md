# Architecture

## Clean Architecture .NET 9

RIVORA Framework suit une architecture en couches stricte avec 47 projets organises :

```
Presentation (API)           Controllers, Minimal APIs, gRPC, SignalR, GraphQL
       |
Application (CQRS)          Commands, Queries, Validators, DTOs (MediatR)
       |
Domain (Metier)              Entities, Value Objects, Domain Events, Specifications
       |
Infrastructure (Data)       EF Core 9, Repositories, Services externes
```

**Regle fondamentale** : les dependances pointent toujours vers le centre. Domain ne depend de rien. Infrastructure depend de Domain et Application, jamais l'inverse.

## Structure des projets

| Couche | Projets | Role |
|--------|---------|------|
| **Core** | `Core`, `Domain` | Abstractions, entites, events, specifications |
| **Application** | `Application` | CQRS (MediatR), services, validators, DTOs |
| **Infrastructure** | `Infrastructure`, `Data.*` (4 providers) | EF Core, repositories, services externes |
| **Presentation** | `Api`, `GraphQL`, `Admin`, `RealTime` | REST, GraphQL, Blazor, SignalR |
| **Security** | `Security`, `Identity.Pro` | JWT, BCrypt, 2FA, Rate Limiting, Audit |
| **Multi-Tenancy** | `MultiTenancy`, `SaaS` | Isolation tenant, billing |
| **Cross-Cutting** | `Caching`, `Jobs.*`, `HealthChecks`, `Notifications`, `Storage`, `Features`, `FeatureManagement`, `Localization.Dynamic`, `ApiVersioning` | Modules transversaux |
| **IA** | `AI`, `NaturalQuery` | RAG, vector store, NL Query Builder |
| **Integration** | `Export`, `Webhooks`, `Client` | PDF/Excel/CSV, webhooks SaaS, client API |
| **DevOps** | `AppHost`, `ServiceDefaults` | .NET Aspire orchestration |
| **Outils** | `RVR.CLI`, `RVR.Studio` | CLI scaffolding + AI, IDE visuel |

## Arborescence

```
src/
  RVR.Framework.Core/              # Abstractions de base
  RVR.Framework.Domain/            # Entites, Value Objects, Events
  RVR.Framework.Application/       # CQRS, Services, DTOs
  RVR.Framework.Infrastructure/    # EF Core, Repositories
  RVR.Framework.Api/               # REST API + Swagger
  RVR.Framework.Security/          # JWT, BCrypt, 2FA
  RVR.Framework.MultiTenancy/      # Isolation multi-tenant
  RVR.Framework.Caching/           # Cache L1 (Memory) + L2 (Redis)
  RVR.Framework.Export/            # PDF, Excel, CSV
  RVR.Framework.Webhooks/          # Publish/Subscribe HMAC
  RVR.Framework.GraphQL/           # HotChocolate Gateway
  RVR.Framework.AI/                # RAG, LLM integration
  RVR.Framework.NaturalQuery/      # Langage naturel -> LINQ
  RVR.Framework.Billing/           # Facturation SaaS
  RVR.Framework.Data.SqlServer/    # Provider SQL Server
  RVR.Framework.Data.PostgreSQL/   # Provider PostgreSQL
  RVR.Framework.Data.MySQL/        # Provider MySQL
  RVR.Framework.Data.SQLite/       # Provider SQLite
  ...
tests/
  RVR.Framework.Core.Tests/
  RVR.Framework.Application.Tests/
  RVR.Framework.Domain.Tests/
  RVR.Framework.Api.IntegrationTests/
  ...
tools/
  RVR.CLI/                         # CLI scaffolding + AI review
  RVR.Studio/                      # IDE visuel
```

## Modularite

Chaque module implemente `IRvrModule` pour un enregistrement standardise :

```csharp
public class CachingModule : IRvrModule
{
    public string Name => "Caching";
    public string Version => "3.0.0";

    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddRvrCaching(config);
    }
}
```

Enregistrement dans `Program.cs` :

```csharp
builder.Services.AddRvrModule<CachingModule>(builder.Configuration);
builder.Services.AddRvrModule<SecurityModule>(builder.Configuration);
builder.Services.AddRvrModule<MultiTenancyModule>(builder.Configuration);
```

Cette approche permet de passer d'un monolithe modulaire a des microservices sans refactoring majeur.
