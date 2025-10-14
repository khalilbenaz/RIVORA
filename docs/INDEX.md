# Documentation RIVORA Framework

> **Version** : 3.2.0 | **Target** : .NET 9.0 (C# 13) | **Licence** : MIT
> **Derniere mise a jour** : 2026-03-17

## Guides de demarrage
- **[quickstart.md](quickstart.md)** - Demarrage en 5 minutes
- **[GUIDE_TEST_RAPIDE.md](GUIDE_TEST_RAPIDE.md)** - Guide de test rapide
- **[INSTALLATION_PACKAGES.md](INSTALLATION_PACKAGES.md)** - Installation des packages NuGet
- **[TUTORIAL-STEP-BY-STEP.md](TUTORIAL-STEP-BY-STEP.md)** - Tutoriel complet pas-a-pas
- **[INITIALIZATION-GUIDE.md](INITIALIZATION-GUIDE.md)** - Guide d'initialisation

## Architecture & Conception
- **[project-context.md](project-context.md)** - Contexte projet complet (architecture, stack, modules)
- **[GUIDE-COMPLET.md](GUIDE-COMPLET.md)** - Guide detaille de l'architecture et patterns
- **[MIGRATION.md](MIGRATION.md)** - Guide de migration entre versions

## Securite & Multi-tenancy
- **[AUTHORIZATION_SUMMARY.md](AUTHORIZATION_SUMMARY.md)** - Systeme d'autorisation RBAC + JWT
- **[TENANTID_IMPLEMENTATION.md](TENANTID_IMPLEMENTATION.md)** - Implementation multi-tenancy

## Modules
- **[modules/](modules/)** - Documentation detaillee par module

## CLI
- **[cli/](cli/)** - Commandes et utilisation de `rvr`

## Rapports & Ameliorations
- **[AMELIORATIONS_IMPLEMENTEES.md](AMELIORATIONS_IMPLEMENTEES.md)** - Historique des ameliorations
- **[RAPPORT-ANOMALIES-AMELIORATIONS.md](RAPPORT-ANOMALIES-AMELIORATIONS.md)** - Anomalies corrigees + propositions v3.0

## Plans
- **[plans/](plans/)** - Plans et roadmaps de developpement

---

## Structure du Projet (35+ projets src + 11 tests + 2 outils = 55+ projets)

```
RVR.Framework/
|-- src/
|   |-- RVR.Framework.Core/               # Abstractions, interfaces, base classes
|   |-- RVR.Framework.Domain/             # Entites, Value Objects, Events
|   |-- RVR.Framework.Application/        # CQRS, Services, Validators, DTOs
|   |-- RVR.Framework.Infrastructure/     # EF Core, Repositories, Services externes
|   |-- RVR.Framework.Api/                # Controllers, Middleware, Endpoints
|   |-- RVR.Framework.Security/           # BCrypt, JWT, 2FA, Audit, Rate Limiting
|   |-- RVR.Framework.MultiTenancy/       # Isolation tenant, filtres EF Core
|   |-- RVR.Framework.Identity.Pro/       # Fonctionnalites identite avancees
|   |-- RVR.Framework.Data.Abstractions/  # Abstractions data layer
|   |-- RVR.Framework.Data.SqlServer/     # Provider SQL Server
|   |-- RVR.Framework.Data.PostgreSQL/    # Provider PostgreSQL
|   |-- RVR.Framework.Data.MySQL/         # Provider MySQL
|   |-- RVR.Framework.Data.SQLite/        # Provider SQLite
|   |-- RVR.Framework.Caching/            # Cache 2 niveaux (Memory + Redis)
|   |-- RVR.Framework.Jobs.Abstractions/  # Interface IJobScheduler
|   |-- RVR.Framework.Jobs.Hangfire/      # Integration Hangfire
|   |-- RVR.Framework.Jobs.Quartz/        # Integration Quartz.NET
|   |-- RVR.Framework.HealthChecks/       # 80+ health checks
|   |-- RVR.Framework.RealTime/           # SignalR hubs multi-tenant
|   |-- RVR.Framework.Notifications/      # Email, Push, SMS
|   |-- RVR.Framework.ApiVersioning/      # Versioning strategies
|   |-- RVR.Framework.Features/           # Feature gates & toggles
|   |-- RVR.Framework.FeatureManagement/  # Feature management avance
|   |-- RVR.Framework.Storage/            # Abstraction stockage fichiers
|   |-- RVR.Framework.Localization.Dynamic/ # Localisation dynamique
|   |-- RVR.Framework.SaaS/              # Fonctionnalites SaaS (Stripe, billing)
|   |-- RVR.Framework.AuditLogging.UI/   # UI pour audit logs
|   |-- RVR.Framework.Privacy/           # GDPR Privacy Toolkit
|   |-- RVR.Framework.EventSourcing/     # Event Sourcing (IAggregateRoot, IEventStore)
|   |-- RVR.Framework.Saga/             # Saga / Process Manager
|   |-- RVR.Framework.AppHost/           # .NET Aspire host
|   |-- RVR.Framework.ServiceDefaults/   # Defaults Aspire
|
|-- tests/ (11 projets)
|-- tools/
|   |-- RVR.CLI/                          # CLI `rvr` (scaffolding, AI, diagnostics)
|   |-- RVR.Studio/                       # IDE visuel pour modelisation
|-- samples/
|   |-- ai-rag-app/                       # Exemple RAG avec IA
|   |-- microservices-demo/               # Exemple microservices + gRPC
|   |-- saas-starter/                     # Template SaaS complet
```

## Technologies Principales

| Categorie | Technologies |
|-----------|-------------|
| Runtime | .NET 9.0, C# 13 |
| ORM | Entity Framework Core 9.0 |
| CQRS | MediatR 12.2 |
| Validation | FluentValidation 11.9 |
| Logging | Serilog 4.2 + OpenTelemetry 1.9 |
| Cache | Redis (StackExchange.Redis 2.8) + Memory |
| Messaging | MassTransit 8.1 + RabbitMQ |
| Jobs | Hangfire 1.8 + Quartz.NET 3.13 |
| Securite | BCrypt, JWT, OAuth2/OIDC, TOTP/2FA, GDPR, Rate Limiting |
| Monitoring | Prometheus + Grafana |
| Tests | xUnit 2.9, Moq, FluentAssertions 7.0, Bogus |
| AI | Anthropic SDK 4.2, OpenAI 2.2 |
