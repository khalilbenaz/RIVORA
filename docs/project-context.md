# RIVORA Framework - Contexte Projet

> Document genere le 2026-03-17 | Version 3.2.0 | .NET 9.0

## 1. Presentation

RIVORA Framework est un framework enterprise-grade pour .NET 9 concu pour construire des applications SaaS scalables et securisees. Il fournit une fondation complete incluant :

- **Clean Architecture** (Domain-Driven Design)
- **Multi-tenancy** (isolation par schema, base, ou filtrage)
- **Securite avancee** (JWT, BCrypt, OAuth2/OIDC, 2FA TOTP, GDPR toolkit, Rate Limiting, Audit Trail)
- **Observabilite** (OpenTelemetry, Serilog, Prometheus, Grafana)
- **CQRS** via MediatR
- **Outbox Pattern** pour la coherence des evenements
- **Event Sourcing** et **Saga/Process Manager**
- **Modules pluggables** via `IRvrModule`

## 2. Architecture

### 2.1 Couches (Clean Architecture)

```
Presentation (API)          <- Controllers, Minimal APIs, gRPC, SignalR
       |
Application (CQRS)         <- Commands, Queries, Validators, DTOs
       |
Domain (Metier)             <- Entities, Value Objects, Domain Events, Specifications
       |
Infrastructure (Data)      <- EF Core, Repositories, Services externes
```

### 2.2 Principes

- **Dependency Inversion** : Les couches internes ne dependent jamais des couches externes
- **Repository Pattern** : `IRepository<T, TId>` generique + repositories specifiques
- **Specification Pattern** : Requetes composables via `ISpecification<T>`
- **Result Pattern** : `Result<T>` pour le Railway-Oriented Programming
- **Outbox Pattern** : Coherence eventuelle via `OutboxMessage`
- **Soft Delete** : Via `ISoftDelete` avec filtre global EF Core

### 2.3 Multi-Tenancy

- `ITenantProvider` pour la resolution du tenant (HTTP header, claim JWT, sous-domaine)
- Filtre global EF Core sur `TenantId` pour isolation automatique
- Support 3 strategies : filtrage par colonne, schema separe, base separee
- `TenantId` injecte dans les claims JWT

### 2.4 Securite

| Fonctionnalite | Implementation |
|----------------|----------------|
| Authentification | JWT Bearer tokens |
| Hachage mots de passe | BCrypt (work factor 12) |
| 2FA | TOTP via QR Code (QRCoder + Otp.NET) |
| Rate Limiting | Middleware custom + ASP.NET Core Rate Limiter |
| Chiffrement donnees | AES-256 via `[EncryptedAtRest]` attribute |
| Audit Trail | EF Core SaveChanges interceptor |
| API Keys | Middleware d'authentification par cle API |
| En-tetes securite | OWASP headers (CSP, HSTS, X-Frame-Options) |
| CORS | Restrictif en production, permissif en dev |
| OAuth2 / OIDC | Azure AD, Keycloak, Auth0 avec claims transformer |
| GDPR Privacy | `[PersonalData]`, DSAR, consent management, data anonymizer |

### 2.5 Observabilite

- **Tracing** : OpenTelemetry avec export OTLP (Jaeger/Zipkin)
- **Metrics** : OpenTelemetry Runtime + ASP.NET + HTTP Client
- **Logging** : Serilog structure avec enrichissement tenant/user
- **Health Checks** : 80+ checks (databases, cloud, messaging, etc.)
- **Dashboards** : Grafana pre-configure (`/infra/grafana/`)

## 3. Modules

### 3.1 Core (`RVR.Framework.Core`)
Interfaces de base, `Entity<T>`, `AggregateRoot<T>`, `IRepository<T,TId>`, attributs (`[EncryptedAtRest]`), `IRvrModule`.

### 3.2 Domain (`RVR.Framework.Domain`)
Entites metier : `User`, `Product`, `Role`, `Permission`, `Tenant`, `AuditLog`, `BackgroundJob`, `OutboxMessage`. DTOs et Result pattern.

### 3.3 Application (`RVR.Framework.Application`)
Services : `IAuthService`, `IUserService`, `IProductService`. Validators FluentValidation. CQRS via MediatR.

### 3.4 Infrastructure (`RVR.Framework.Infrastructure`)
`RVRDbContext`, repositories EF Core, `JwtTokenService`, `AuthService`. Configurations EF Core avec Fluent API.

### 3.5 Security (`RVR.Framework.Security`)
`PasswordHasherService` (BCrypt), `TotpService` (2FA), `RateLimitService`, `AuditTrailInterceptor`, `RefreshTokenService`.

### 3.6 Data Providers
4 providers : SQL Server, PostgreSQL, MySQL, SQLite. Chacun avec ses optimisations specifiques.

### 3.7 Caching (`RVR.Framework.Caching`)
Cache 2 niveaux : Memory (L1) + Redis (L2). Serialisation MessagePack pour performance. TTL configurable.

### 3.8 Jobs
`IJobScheduler` unifie. Implementations : Hangfire (dashboard web) et Quartz.NET (scheduling avance, CRON).

### 3.9 RealTime (`RVR.Framework.RealTime`)
Hubs SignalR multi-tenant. Notifications en temps reel avec filtrage par tenant/user.

### 3.10 Notifications (`RVR.Framework.Notifications`)
Email (SMTP), Push notifications, templates configurables.

### 3.11 HealthChecks
80+ health checks couvrant : bases de donnees (SQL Server, PostgreSQL, MySQL, MongoDB, Redis, Cosmos, etc.), cloud (AWS, Azure, GCP), messaging (RabbitMQ, Kafka, Nats), infrastructure (Docker, Kubernetes, Consul).

### 3.12 CLI (`rvr`)
Outil de scaffolding et productivite :
- `rvr new` - Templates de projets
- `rvr generate` - Generation de code (CRUD, CQRS, aggregats)
- `rvr ai chat/generate/review` - Assistance IA (Claude, OpenAI)
- `rvr doctor` - Diagnostics projet
- `rvr generate client` - OpenAPI -> typed C# client
- `rvr migrate generate/apply/list/rollback` - EF Core migration management

### 3.13 Privacy (`RVR.Framework.Privacy`)
GDPR Privacy Toolkit : `[PersonalData]` attribute, DSAR (Data Subject Access Request), consent management, data anonymizer.

### 3.14 EventSourcing (`RVR.Framework.EventSourcing`)
Event Sourcing module : `IAggregateRoot`, `IEventStore`, `InMemoryEventStore`. Append-only event streams with snapshot support.

### 3.15 Saga (`RVR.Framework.Saga`)
Saga/Process Manager : `ISaga`, `SagaOrchestrator`, `InMemorySagaStore`. Orchestration de processus metier distribues avec compensation.

### 3.16 Localization.Dynamic (`RVR.Framework.Localization.Dynamic`)
Dynamic Localization : DB-driven translations, `IStringLocalizer` integration, hot reload sans redemarrage.

### 3.17 AuditLogging.UI (`RVR.Framework.AuditLogging.UI`)
Dashboard audit : timeline, filtres avances, export CSV/PDF.

## 4. Configuration

### 4.1 Points d'entree
- **HTTP** : `http://localhost:5220`
- **HTTPS** : `https://localhost:7285`
- **Swagger** : `/swagger`
- **ReDoc** : `/api-docs`
- **Health** : `/health`, `/health/live`, `/health/ready`

### 4.2 Base de donnees
Par defaut : SQL Server LocalDB. Configurable via `ConnectionStrings:DefaultConnection`.

### 4.3 JWT
Configure dans `appsettings.json` section `JwtSettings` : SecretKey, Issuer, Audience, ExpirationMinutes.

## 5. CI/CD

Pipeline GitHub Actions (`ci.yml`) :
1. **Build** - Matrice multi-OS (Ubuntu, Windows, macOS) en Release
2. **Test** - Tests unitaires avec couverture (seuil 70%)
3. **Code Quality** - Format check (`dotnet format`)
4. **Security** - Audit NuGet vulnerabilites
5. **Publish** - Push NuGet sur tags `v*`

## 6. Samples

3 applications exemple :
- **ai-rag-app** : Application RAG avec Blazor UI
- **microservices-demo** : Microservices avec API Gateway + gRPC
- **saas-starter** : Template SaaS complet avec Stripe
