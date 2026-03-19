# Documentation RIVORA Framework

> **Version** : 4.0.0 | **Target** : .NET 9.0 (C# 13) | **Licence** : MIT
> **Derniere mise a jour** : 2026-03-19

## Guides de demarrage
- **[quickstart.md](quickstart.md)** - Demarrage en 5 minutes (backend + frontend + wizard)
- **[GUIDE_TEST_RAPIDE.md](GUIDE_TEST_RAPIDE.md)** - Guide de test rapide
- **[INSTALLATION_PACKAGES.md](INSTALLATION_PACKAGES.md)** - Installation des packages NuGet
- **[TUTORIAL-STEP-BY-STEP.md](TUTORIAL-STEP-BY-STEP.md)** - Tutoriel complet pas-a-pas
- **[INITIALIZATION-GUIDE.md](INITIALIZATION-GUIDE.md)** - Guide d'initialisation

## Architecture & Conception
- **[project-context.md](project-context.md)** - Contexte projet complet (architecture, stack, modules)
- **[ARCHITECTURE.md](ARCHITECTURE.md)** - Diagrammes C4, dependances, flux HTTP
- **[GUIDE-COMPLET.md](GUIDE-COMPLET.md)** - Guide detaille de l'architecture et patterns
- **[NATIVE-AOT.md](NATIVE-AOT.md)** - Compatibilite Native AOT (89% des modules)
- **[MIGRATION.md](MIGRATION.md)** - Guide de migration entre versions

## Securite & Multi-tenancy
- **[AUTHORIZATION_SUMMARY.md](AUTHORIZATION_SUMMARY.md)** - Systeme d'autorisation RBAC + JWT
- **[TENANTID_IMPLEMENTATION.md](TENANTID_IMPLEMENTATION.md)** - Implementation multi-tenancy

## Modules
- **[modules/](modules/)** - Documentation detaillee par module

## CLI
- **[cli/](cli/)** - Commandes et utilisation de `rvr`

## Front End React (v4.0)
- **28 pages** : Landing, App client SaaS, Back Office admin
- **Outils** : Flow Builder, Project Wizard, Entity Generator, Kanban Board
- **Features** : Chat temps reel, Analytics, Calendar, Notes, File Manager, Activity Feed
- **Webhooks** : Incoming/Outgoing, Testing UI, Visual Builder
- **UX** : Dark mode, PWA, i18n FR/EN, push notifications, CSV export, pagination
- **Tests** : 41 unit tests (Vitest), 20 E2E (Playwright)
- **Stack** : React 19, TypeScript, Vite 6, TailwindCSS 4, Zustand, Axios, SignalR

## Rapports & Ameliorations
- **[AMELIORATIONS_IMPLEMENTEES.md](AMELIORATIONS_IMPLEMENTEES.md)** - Historique des ameliorations
- **[RAPPORT-ANOMALIES-AMELIORATIONS.md](RAPPORT-ANOMALIES-AMELIORATIONS.md)** - Anomalies corrigees + propositions

## Plans
- **[plans/](plans/)** - Plans et roadmaps de developpement

---

## Structure du Projet (70+ projets src + 11 tests + 2 outils + frontend React)

```
RIVORA/
|-- src/
|   |-- core/
|   |   |-- RVR.Framework.Core/               # Abstractions, interfaces, base classes
|   |   |-- RVR.Framework.Domain/             # Entites, Value Objects, Events
|   |   |-- RVR.Framework.Application/        # CQRS, Services, Validators, DTOs
|   |   |-- RVR.Framework.Infrastructure/     # EF Core, Repositories, Services externes
|   |-- api/
|   |   |-- RVR.Framework.Api/                # Controllers, Middleware, Endpoints
|   |   |-- RVR.Framework.ApiVersioning/      # Versioning strategies
|   |   |-- RVR.Framework.GraphQL/            # HotChocolate GraphQL
|   |-- security/
|   |   |-- RVR.Framework.Security/           # BCrypt, JWT, 2FA, Audit, Rate Limiting
|   |   |-- RVR.Framework.Identity.Pro/       # Identity avancee, sessions
|   |   |-- RVR.Framework.Privacy/            # GDPR Privacy Toolkit
|   |-- data/
|   |   |-- RVR.Framework.Data.SqlServer/     # Provider SQL Server
|   |   |-- RVR.Framework.Data.PostgreSQL/    # Provider PostgreSQL
|   |   |-- RVR.Framework.Data.MySQL/         # Provider MySQL
|   |   |-- RVR.Framework.Data.SQLite/        # Provider SQLite
|   |   |-- RVR.Framework.Data.MongoDB/       # Provider MongoDB
|   |   |-- RVR.Framework.Data.CosmosDB/      # Provider CosmosDB
|   |-- modules/
|   |   |-- RVR.Framework.Caching/            # Cache 2 niveaux (Memory + Redis)
|   |   |-- RVR.Framework.Jobs.*/             # Hangfire + Quartz.NET
|   |   |-- RVR.Framework.Email/              # SMTP + templates HTML
|   |   |-- RVR.Framework.SMS/               # Twilio, Vonage, OVH, Azure
|   |   |-- RVR.Framework.Features/           # Feature gates & toggles
|   |   |-- RVR.Framework.HealthChecks/       # 80+ health checks
|   |   |-- RVR.Framework.Plugins/            # NuGet plugin system
|   |   |-- RVR.Framework.RealTime/           # SignalR hubs multi-tenant
|   |   |-- RVR.Framework.Profiling/          # MiniProfiler
|   |-- multitenancy/
|   |   |-- RVR.Framework.MultiTenancy/       # Isolation tenant
|   |   |-- RVR.Framework.SaaS/              # Billing Stripe, onboarding
|   |-- integration/
|   |   |-- RVR.Framework.Export/             # PDF, Excel, CSV
|   |   |-- RVR.Framework.Webhooks/           # Outgoing + Incoming webhooks
|   |   |-- RVR.Framework.Client/            # Client API type
|   |-- ai/
|   |   |-- RVR.Framework.AI/                # RAG, vector store
|   |   |-- RVR.Framework.AI.Agents/         # Multi-agent orchestration
|   |   |-- RVR.Framework.NaturalQuery/      # NL → LINQ
|   |-- ui/
|       |-- RVR.Framework.Admin/             # Blazor admin (legacy)
|
|-- frontend/                                # React 19 + TypeScript (28 pages)
|   |-- src/pages/                           # Landing, Chat, Analytics, Calendar, etc.
|   |-- src/pages/flows/                     # Flow Builder (canvas + pipeline)
|   |-- src/pages/projects/                  # Project Wizard + templates
|   |-- src/pages/generator/                 # Entity/CRUD code generator
|
|-- tests/ (11 projets + 41 frontend + 20 E2E)
|-- tools/
|   |-- RVR.CLI/                             # CLI `rvr` (scaffolding, AI, diagnostics)
|   |-- RVR.Studio/                          # IDE visuel Blazor
|-- infra/                                   # Docker, Grafana, Prometheus, Loki
|-- samples/                                 # ai-rag-app, microservices-demo, saas-starter
```

## Technologies Principales

| Categorie | Technologies |
|-----------|-------------|
| Runtime | .NET 9.0, C# 13 |
| Frontend | React 19, TypeScript 5.7, Vite 6, TailwindCSS 4 |
| ORM | Entity Framework Core 9.0 |
| CQRS | MediatR 12.2 |
| Validation | FluentValidation 11.9 |
| Logging | Serilog 4.2 + OpenTelemetry 1.9 |
| Cache | Redis (StackExchange.Redis 2.8) + Memory |
| Messaging | MassTransit 8.1 + RabbitMQ |
| Jobs | Hangfire 1.8 + Quartz.NET 3.13 |
| Securite | BCrypt, JWT 8.3, OAuth2/OIDC, TOTP/2FA, GDPR, Rate Limiting |
| Real-time | SignalR (multi-tenant hubs) |
| Monitoring | Prometheus + Grafana + OpenTelemetry |
| Tests Backend | xUnit 2.9, Moq, FluentAssertions 7.0 |
| Tests Frontend | Vitest 3.2, Testing Library, Playwright 1.49 |
| AI | Anthropic SDK 4.2, OpenAI 2.2, Ollama |
| State | Zustand 5.0 |
| HTTP | Axios 1.7 (retry, interceptors) |
| i18n | i18next + react-i18next |
