# RIVORA Framework

![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&style=flat-square)
![License](https://img.shields.io/badge/license-MIT-green?style=flat-square)
![Build](https://img.shields.io/badge/build-passing-brightgreen?style=flat-square)
![Coverage](https://img.shields.io/badge/coverage-93%25-success?style=flat-square)
![Version](https://img.shields.io/badge/version-4.0.0-blue?style=flat-square)
[![Open in GitHub Codespaces](https://img.shields.io/badge/Open_in-Codespaces-blue?logo=github&style=flat-square)](https://codespaces.new/khalilbenaz/RIVORA)

**Framework d'entreprise Cloud-Native pour .NET 9 - Clean Architecture, DDD et Multi-tenancy pour applications SaaS professionnelles.**

## A propos de RIVORA Framework

RIVORA Framework est un **accelerateur de developpement d'entreprise** complet concu pour les applications SaaS et Cloud-Native sous .NET 9.

**Pourquoi choisir RIVORA Framework ?**
- **Securite "By Design"** : Isolation multi-tenant, BCrypt, 2FA/TOTP, AES-256, OAuth2/OIDC, GDPR toolkit, account lockout, rate limiting, OWASP headers.
- **Performance Extreme** : Compiled queries centralisees, DbContext Pooling, cache 2 niveaux (Memory/Redis), ETag caching, Keyset pagination, gRPC.
- **Modularite Absolue** : Architecture monolithique modulaire (`IRvrModule`) scalable vers microservices, Event Sourcing, Saga/Process Manager.
- **DX Inegalee** : RVR Studio (IDE visuel), RVR.CLI (scaffolding + AI review + migrations), client API type, GraphQL gateway.
- **IA Integree** : Module RAG, NL Query Builder, AI code review (Architecture, DDD, Performance, Security analyzers).

**Nouveau dans la v4.0.0** : 28 pages React (Landing + SaaS + Back Office), Flow Builder visuel (node editor + pipeline), Project Wizard (6 templates), Entity/CRUD Generator, Kanban Board, Chat temps reel, Analytics Dashboard, Calendar, Notes, File Manager, Incoming Webhooks avec signature validation, 89% Native AOT, 93 tests (52 backend + 41 frontend), PWA, i18n FR/EN, dark mode.

---

## Front End (React + TypeScript)

Le projet inclut un **front end complet** en React 19 + TypeScript + TailwindCSS 4 + Vite :

### Pages publiques
| Route | Page |
|-------|------|
| `/` | Landing page (hero, features, pricing, temoignages, CTA) |
| `/app/login` | Connexion client |
| `/app/register` | Inscription client |
| `/components` | Showcase de tous les composants UI |

### App client SaaS
| Route | Page |
|-------|------|
| `/app` | Dashboard client (quick actions, getting started) |
| `/app/settings` | Profil, securite/2FA, cles API, notifications push |

### Back Office admin (28 pages)
| Route | Page |
|-------|------|
| `/admin` | Dashboard (stats, audit logs recents) |
| `/admin/users` | CRUD Utilisateurs + CSV export |
| `/admin/products` | CRUD Produits + CSV export |
| `/admin/tenants` | Gestion Tenants (cards) |
| `/admin/audit` | Audit Logs (filtres, pagination, CSV) |
| `/admin/health` | Health monitoring (auto-refresh 30s) |
| `/admin/roles` | Roles & Permissions (matrice) |
| `/admin/webhooks` | Webhooks (sortant + entrant + builder visuel) |
| `/admin/flows` | Flow Builder (liste des flows) |
| `/admin/flows/new` | Flow Editor (canvas visuel + pipeline) |
| `/admin/chat` | Chat temps reel (rooms, messages, SignalR) |
| `/admin/files` | File Manager (drag & drop, thumbnails, dossiers) |
| `/admin/analytics` | Analytics Dashboard (graphiques SVG) |
| `/admin/calendar` | Calendrier d'evenements (grille mensuelle) |
| `/admin/notes` | Notes (masonry grid, pin, 5 couleurs) |
| `/admin/activity` | Fil d'activite (timeline, 8 types, filtres) |
| `/admin/kanban` | Kanban Board (drag & drop, 5 colonnes) |
| `/admin/projects` | Mes Projets (liste, statut) |
| `/admin/projects/new` | Project Wizard (6 etapes, 6 templates) |
| `/admin/generator` | Entity/CRUD Generator (designer + code preview) |

```bash
cd frontend
npm install
npm run dev    # http://localhost:3000 (proxy vers API .NET)
```

**Stack** : React 19, TypeScript, Vite 6, TailwindCSS 4, Zustand, Axios, Lucide Icons, i18next, SignalR, Vitest, Playwright.

**Features** : Dark mode, PWA, i18n FR/EN, push notifications, session auto-refresh, toast notifications, CSV export, pagination, skeleton loading, ErrorBoundary, retry 429/503, WCAG accessibility.

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
- **Front End** : `http://localhost:3000` (React SPA - `cd frontend && npm run dev`)

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
| AI Guardrails | Prompt injection detection, PII masking, content moderation, token budget |
| AI Agents | Orchestration multi-agents, ReAct strategy, pipeline sequentiel/parallele |
| CLI AI Review | Architecture, DDD, Performance, Security analyzers |
| LLM Backends | OpenAI, Claude, Ollama (offline) |
| SARIF Output | Integration CI/CD pipelines |

### Multi-Tenancy & SaaS
| Feature | Description |
|---------|-------------|
| Tenant Isolation | 3 strategies (colonne, schema, base separee) |
| Tenant Onboarding | Provisioning automatise avec rollback Saga |
| Cross-Tenant Analytics | Dashboard super-admin, metriques, export CSV |
| SaaS Billing | Stripe subscriptions, checkout, portal |

### Modules additionnels
| Feature | Description |
|---------|-------------|
| SMS multi-provider | Twilio, Vonage, OVH, Azure Communication Services |
| Plugin System | NuGet auto-discovery, signature verification, marketplace |
| .NET Aspire | Integration native, orchestration complète |

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

## Quick Start with Codespaces

[![Open in GitHub Codespaces](https://github.com/codespaces/badge.svg)](https://codespaces.new/khalilbenaz/RIVORA)

Click the button above to get a pre-configured development environment with .NET 9 SDK and RVR CLI installed.

---

## RVR CLI

```bash
# Interactive wizard (recommended)
rvr new

# Or with flags
rvr new MySaaS --template saas-starter
rvr generate crud Invoice --props "Reference:string,Amount:decimal"
rvr add-module Inventory
rvr remove-module Caching                     # Retire proprement un module
rvr remove-module Caching --dry-run           # Previsualiser les changements

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

# Client & Test Generation
rvr generate client                           # OpenAPI -> typed C# client
rvr generate test Invoice                     # xUnit + FluentAssertions tests
rvr generate seed Product                     # Scaffolder un data seeder

# Database
rvr migrate generate MigrationName            # Generate EF Core migration
rvr migrate apply                             # Apply pending migrations
rvr migrate list                              # List migrations
rvr migrate rollback                          # Rollback last migration
rvr seed --profile demo                       # Seeder la base de donnees
rvr seed --reset --profile test               # Truncate + reseed

# Environnements & Secrets
rvr env list                                  # Lister les environnements
rvr env set DB:Host "localhost"               # Definir une variable
rvr env diff Development Production           # Comparer deux envs
rvr env secrets init                          # Initialiser User Secrets
rvr env export --format dotenv                # Exporter en .env
rvr env import --file .env                    # Importer depuis .env

# Publication
rvr publish --target docker                   # Build + push Docker
rvr publish --target nuget                    # Pack + push NuGet
rvr publish --target self-contained           # Binaires autonomes
rvr publish --dry-run                         # Previsualiser les commandes

# Upgrade
rvr upgrade --list                            # Migrations disponibles
rvr upgrade --to 4.0 --dry-run               # Previsualiser la migration
rvr upgrade --to 4.0                          # Migrer vers v4.0

# DevOps
rvr doctor
rvr benchmark
```

---

## Telecharger RVR Studio Desktop

| Plateforme | Fichier | Format |
|------------|---------|--------|
| **Windows** (x64) | [RVR-Studio-Setup-win-x64.zip](https://github.com/khalilbenaz/RIVORA/releases/latest/download/RVR-Studio-Setup-win-x64.zip) | ZIP |
| **macOS** (x64) | [RVR-Studio-Setup-macos.tar.gz](https://github.com/khalilbenaz/RIVORA/releases/latest/download/RVR-Studio-Setup-macos.tar.gz) | tar.gz |
| **Linux** (x64) | [RVR-Studio-linux-x64.tar.gz](https://github.com/khalilbenaz/RIVORA/releases/latest/download/RVR-Studio-linux-x64.tar.gz) | tar.gz |

> Prerequis : [.NET 9 Runtime](https://dotnet.microsoft.com/download/dotnet/9.0) installe sur la machine.

> Tous les installeurs sont disponibles sur la page [Releases](https://github.com/khalilbenaz/RIVORA/releases).

---

## Documentation

| Guide | Contenu |
|-------|---------|
| [Quickstart](docs/quickstart.md) | Installation en 5 minutes |
| [Creer son projet](website/guide/create-project.md) | Guide pas-a-pas pour demarrer avec RIVORA |
| [Architecture (C4)](docs/ARCHITECTURE.md) | Diagrammes C4, dependances, flux HTTP |
| [Native AOT](docs/NATIVE-AOT.md) | Audit AOT, patterns, compatibilite |
| [Guide Complet](docs/GUIDE-COMPLET.md) | Architecture et patterns detailles |
| [Project Context](docs/project-context.md) | Vue d'ensemble technique complete |
| [Modules](docs/modules/) | Documentation par module |
| [CLI](docs/cli/) | Reference commandes CLI |
| [Site web](https://khalilbenaz.github.io/RIVORA/) | Documentation complete en ligne |

---

## Known Limitations

- **Native AOT**: 14 modules compatibles, 27 partiellement, 15 non-compatibles. Voir [docs/NATIVE-AOT.md](docs/NATIVE-AOT.md) pour le detail.
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
