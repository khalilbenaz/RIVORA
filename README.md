# RIVORA Framework

![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&style=flat-square)
![License](https://img.shields.io/badge/license-MIT-green?style=flat-square)
![Build](https://img.shields.io/badge/build-passing-brightgreen?style=flat-square)
![Coverage](https://img.shields.io/badge/coverage-85%25-success?style=flat-square)
![Version](https://img.shields.io/badge/version-3.3.0-blue?style=flat-square)

**Framework d'entreprise Cloud-Native pour .NET 9 - Clean Architecture, DDD et Multi-tenancy pour applications SaaS professionnelles.**

## A propos de RIVORA Framework

RIVORA Framework est un **accelerateur de developpement d'entreprise** complet concu pour les applications SaaS et Cloud-Native sous .NET 9.

**Pourquoi choisir RIVORA Framework ?**
- **Securite "By Design"** : Isolation multi-tenant, BCrypt, 2FA/TOTP, AES-256, OAuth2/OIDC, GDPR toolkit, account lockout, rate limiting, OWASP headers.
- **Performance Extreme** : Compiled queries centralisees, DbContext Pooling, cache 2 niveaux (Memory/Redis), ETag caching, Keyset pagination, gRPC.
- **Modularite Absolue** : Architecture monolithique modulaire (`IRvrModule`) scalable vers microservices, Event Sourcing, Saga/Process Manager.
- **DX Inegalee** : RVR Studio (IDE visuel), RVR.CLI (scaffolding + AI review + migrations), client API type, GraphQL gateway.
- **IA Integree** : Module RAG, NL Query Builder, AI code review (Architecture, DDD, Performance, Security analyzers).

**Nouveau dans la v3.2.0** : OAuth2/OIDC (Azure AD, Keycloak, Auth0), GDPR Privacy Toolkit, Event Sourcing, Saga/Process Manager, Keyset pagination, Tenant Lifecycle Management, Dynamic Localization, AuditLogging.UI, Value Objects enrichis, `rvr generate client`, `rvr migrate` commands.

---

## Demarrage Rapide

### Prerequis
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- SQL Server, PostgreSQL, MySQL ou SQLite
- (Optionnel) Docker pour l'environnement de dev complet

### Installation
```bash
# 1. Cloner
git clone https://github.com/khalilbenaz/RIVORA.git
cd RVR.Framework

# 2. Restaurer et Lancer
dotnet restore
dotnet run --project src/api/RVR.Framework.Api

# 3. (Optionnel) Lancer l'environnement dev complet
docker compose -f docker-compose.dev.yml up -d
```

### Endpoints
- **API** : `http://localhost:5220`
- **Swagger** : `http://localhost:5220/swagger`
- **ReDoc** : `http://localhost:5220/api-docs`
- **Health** : `http://localhost:5220/health`
- **Admin** : `http://localhost:5200` (RVR.Framework.Admin)

---

## Architecture

### Clean Architecture .NET 9 (70+ projets)

```
Presentation (API)           Controllers, Minimal APIs, gRPC, SignalR, GraphQL
       |
Application (CQRS)          Commands, Queries, Validators, DTOs (MediatR)
       |
Domain (Metier)              Entities, Value Objects, Domain Events, Specifications
       |
Infrastructure (Data)       EF Core 9, Repositories, Services externes
```

### Structure du Projet

| Couche | Projets | Description |
|--------|---------|-------------|
| **Core** | Core, Domain | Abstractions, entites, events, specifications |
| **Application** | Application | CQRS, services, validators, DTOs |
| **Infrastructure** | Infrastructure, Data.* (4 providers) | EF Core, repositories, services |
| **Presentation** | Api, GraphQL, Admin, RealTime | REST, GraphQL, Blazor, SignalR |
| **Security** | Security, Identity.Pro, Privacy | JWT, BCrypt, OAuth2/OIDC, 2FA, GDPR toolkit, Rate Limiting, Audit |
| **Multi-Tenancy** | MultiTenancy, SaaS | Isolation tenant, billing, tenant lifecycle management |
| **Architecture** | EventSourcing, Saga | Event Sourcing, Saga/Process Manager, Unit of Work |
| **Cross-Cutting** | Caching, Jobs.*, HealthChecks, Notifications, Storage, Features, FeatureManagement, Localization.Dynamic, ApiVersioning | Modules transversaux |
| **IA** | AI, NaturalQuery | RAG, vector store, NL Query Builder |
| **Billing** | SaaS Billing (Stripe) | Subscriptions, checkout, portal, usage metering, webhooks |
| **Integration** | Export, Webhooks, Client | PDF/Excel/CSV, webhooks SaaS, client API type |
| **DevOps** | AppHost, ServiceDefaults | .NET Aspire orchestration |
| **Outils** | RVR.CLI, RVR.Studio | CLI scaffolding + AI, IDE visuel |

---

## Features

### Securite
| Feature | Description |
|---------|-------------|
| JWT + Refresh Tokens | Tokens persistants avec rotation et revocation |
| BCrypt Password Hashing | Work factor 12, OWASP compliant |
| Account Lockout | Anti brute-force (5 tentatives, 15 min lockout) |
| 2FA/TOTP | QR Code + backup codes |
| AES-256 Encryption | Chiffrement at-rest via `[EncryptedAtRest]` |
| Rate Limiting | Trusted proxy, IP-based et user-based |
| CORS | Restrictif en production, configurable |
| Audit Trail | Intercepteur EF Core automatique |
| OWASP Headers | CSP, HSTS, X-Frame-Options |
| OAuth2 / OIDC | Azure AD, Keycloak, Auth0 avec claims transformer |
| GDPR Privacy Toolkit | `[PersonalData]`, DSAR, consent management, data anonymizer |
| Session Management | Identity.Pro session tracking, admin impersonation with JWT |

### Performance
| Feature | Description |
|---------|-------------|
| Compiled Queries | Centralisees pour User, Product, RefreshToken |
| ETag Caching | Middleware SHA256, 304 Not Modified |
| Cache 2 niveaux | Memory (L1) + Redis (L2) |
| DbContext Pooling | Reutilisation des instances |
| Bulk Operations | Insert/Update/Delete en masse |
| Output Caching | .NET 9 response caching |
| Keyset Pagination | Cursor-based pagination in IRepository for large datasets |

### IA & Innovation
| Feature | Description |
|---------|-------------|
| Module RAG | Ingestion, chunking, vector search, chat |
| NL Query Builder | "produits actifs prix > 100" -> LINQ (FR/EN) |
| CLI AI Review | Architecture, DDD, Performance, Security analyzers |
| LLM Backends | OpenAI, Claude, Ollama (offline) |
| SARIF Output | Integration CI/CD pipelines |

### Integration
| Feature | Description |
|---------|-------------|
| Export | PDF (QuestPDF), Excel (ClosedXML), CSV |
| Webhooks | Publish/Subscribe, HMAC-SHA256, retry backoff |
| GraphQL | HotChocolate 14.3, filtering, sorting, projection |
| Client API | RvrApiClient type pour consommateurs |

### DevOps
| Feature | Description |
|---------|-------------|
| Docker Compose | 8 services (SQL, PG, Redis, RabbitMQ, Prometheus, Grafana, Jaeger, Seq) |
| Testcontainers | Integration tests avec MsSql + Redis reels |
| CI/CD | GitHub Actions multi-OS, security audit, AI SARIF review, NuGet publish |
| .NET Aspire | Orchestration, service discovery |

---

## RVR CLI

```bash
# Scaffolding
rvr new MySaaS --template saas-starter
rvr generate crud Invoice --props "Reference:string,Amount:decimal"
rvr add-module Inventory

# AI Commands
rvr ai chat --provider claude
rvr ai generate "Create a payment service"
rvr ai review --all                           # Tous les analyzers
rvr ai review --architecture                  # Clean Architecture conformance
rvr ai review --ddd                           # DDD anti-patterns
rvr ai review --performance                   # N+1, missing async, EF anti-patterns
rvr ai review --security                      # Vulnerabilites OWASP
rvr ai review --provider ollama               # AI suggestions (offline)
rvr ai review --output sarif --output-file report.sarif  # CI integration
rvr ai design --provider claude              # Interactive domain design

# Client Generation
rvr generate client                           # OpenAPI -> typed C# client

# Test Generation
rvr generate test Invoice                     # xUnit + FluentAssertions tests
rvr generate test Invoice --output ./tests    # Custom output directory

# Migrations
rvr migrate generate MigrationName            # Generate EF Core migration
rvr migrate apply                             # Apply pending migrations
rvr migrate list                              # List migrations
rvr migrate rollback                          # Rollback last migration

# Benchmarks
dotnet run --project tests/RVR.Framework.Benchmarks -c Release

# DevOps
rvr doctor
rvr benchmark
```

---

## Documentation

| Guide | Contenu |
|-------|---------|
| [Quickstart](docs/quickstart.md) | Installation en 5 minutes |
| [Guide Complet](docs/GUIDE-COMPLET.md) | Architecture et patterns detailles |
| [Project Context](docs/project-context.md) | Vue d'ensemble technique complete |
| [Rapport v3.0](docs/RAPPORT-ANOMALIES-AMELIORATIONS.md) | Anomalies corrigees + features implementees |
| [Modules](docs/modules/) | Documentation par module |
| [CLI](docs/cli/) | Reference commandes CLI |

---

## Known Limitations

- **Native AOT**: Not all modules are fully AOT-compatible yet. EF Core and some reflection-heavy modules (AI, GraphQL) require JIT.
- **AI Review CLI**: Requires an API key for OpenAI or Claude. Ollama can be used offline but with reduced analysis quality.
- **SaaS Billing**: Only Stripe is supported. Other payment providers are on the roadmap.
- **Database Providers**: While 4 providers are supported (SQL Server, PostgreSQL, MySQL, SQLite), advanced features like Event Sourcing are optimized for SQL Server and PostgreSQL.

---

## Credits & Contributeurs

Ce framework est co-developpe avec l'assistance de **Claude** (Anthropic AI).

Les contributions sont les bienvenues ! Consultez [CONTRIBUTING.md](CONTRIBUTING.md).

---

## License

Distribue sous la licence **MIT**. Voir `LICENSE` pour plus d'informations.

---

**RIVORA Framework Team** - *Production-Ready Architecture for Modern .NET 9 Applications*
