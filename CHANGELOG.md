# Changelog

## [v3.3.2] - 2026-03-18

- chore: add explicit PackageId and IsPackable to all publishable projects (#97) (54b1c55)
- docs: update CHANGELOG.md for v3.3.1 (ae08d6e)


## [v3.3.1] - 2026-03-18

- feat: RIVORA v3.3.0 - resolve all 13 open issues (#84-#96) (7abb367)


Tous les changements notables de RIVORA Framework sont documentés dans ce fichier.

Le format est basé sur [Keep a Changelog](https://keepachangelog.com/fr/1.0.0/),
et ce projet adhère au [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [3.3.0] - 2026-03-17

### ROADMAP 100% Complete - All planned features implemented

#### New Modules
- **HybridCache** : .NET 9 native L1 Memory + L2 Redis with stampede protection
- **Search Engine** : Elasticsearch integration (ISearchService, facets, highlighting)
- **Alerting Framework** : Multi-channel alerts (Slack, Teams, Email, Console)
- **Workflow Engine** : Lightweight state machine (states, transitions, guards, history)
- **Read Replicas** : Round-robin DB connection routing for read scalability
- **MiniProfiler** : EF Core SQL profiling in development
- **Dapr Integration** : Service invocation, pub/sub, state management, secrets
- **Plugin System** : Dynamic assembly loading with lifecycle management
- **Native AOT** : Core + Domain marked AOT-compatible, JSON source generators
- **API Keys** : SHA256 hashed storage, rotation, scoped access
- **Messaging** : IMessageBus abstraction with InMemoryMessageBus
- **Resilience** : Polly v8 (retry, circuit breaker, timeout)
- **Idempotency** : X-Idempotency-Key middleware with response caching
- **Email** : IEmailSender with SmtpEmailSender
- **CosmosDB** : Azure Cosmos DB data provider
- **MongoDB** : MongoDB data provider

#### Security
- SSRF protection on webhook callback URLs
- CSV formula injection protection
- NL parser property whitelist + Take/Skip limits
- Webhook incoming signature validation
- IP allowlist/blocklist per tenant
- ConcurrentDictionary in DatabaseProviderFactory

#### CLI & Studio
- `rvr generate test` scaffolds xUnit tests
- `rvr ai review` SARIF output in CI
- RVR.CLI tool command renamed to `rvr`

#### Samples & CI
- SaaS Starter sample updated to RVR + .NET 9
- CI: 80% coverage gate, unit/integration test separation, AI review SARIF upload

---

## [3.2.0] - 2026-03-17

### Added - All GitHub issues resolved (14 features)

#### Security
- OAuth 2.0 / OpenID Connect (Azure AD, Keycloak, Auth0) with claims transformer
- GDPR Privacy Toolkit ([PersonalData], DSAR, consent management, data anonymizer)
- Identity.Pro: Session management, admin impersonation with JWT

#### Architecture
- Event Sourcing module (IAggregateRoot, IEventStore, InMemoryEventStore)
- Saga / Process Manager (ISaga, SagaOrchestrator, InMemorySagaStore)
- Keyset pagination (cursor-based) in IRepository
- Value Objects: Email, Money, PhoneNumber, Address, DateRange
- Result<T> enrichment: Map, Bind, Match, structured Error
- Unit of Work pattern
- MediatR Pipeline Behaviors (Validation, Logging, Performance, Exception)

#### SaaS & Multi-tenancy
- Tenant Lifecycle Management (provisioning, onboarding, suspension, deletion)

#### Modules
- Dynamic Localization (DB-driven, IStringLocalizer, hot reload)
- AuditLogging.UI dashboard (timeline, filters, export)

#### CLI
- `rvr generate client` - OpenAPI -> typed C# client
- `rvr migrate generate/apply/list/rollback` - EF Core migration management

#### Samples
- ai-rag-app and microservices-demo updated to RVR naming + .NET 9

#### Studio
- Visual Query Builder page with NL input support

---

## [3.1.0] - 2026-03-16

### Added - SaaS Billing Module (`RVR.Framework.Billing`)
- **Stripe Integration** : Checkout, subscriptions, customer portal, payment methods
- **Subscription Plans** : Free/Standard/Premium tiers with feature limits and usage quotas
- **Usage Metering** : Track API calls, storage, or custom metrics per tenant
- **Webhook Handler** : Process invoice.paid, payment_failed, subscription events with signature verification
- **Billing Endpoints** : Minimal API (POST /api/billing/checkout, portal, usage, GET subscription/invoices)
- **Grace Period** : Configurable retry schedule before suspension on failed payments

### Added - Performance Benchmarking Suite
- **BenchmarkDotNet** project with 4 benchmark classes
- **EF Core** : Compiled vs dynamic queries, bulk ops, pooling, tracking
- **Auth** : JWT generation/validation, BCrypt work factors (10/12/14)
- **CQRS** : MediatR dispatch, FluentValidation, Result pattern overhead
- **Serialization** : System.Text.Json vs MessagePack (small/large objects)
- **CI Integration** : GitHub Actions workflow with regression detection (>10% threshold)

### Added - AI Domain Design (`rvr ai design`)
- Interactive LLM-based domain modeling in natural language
- Identifies entities, value objects, aggregate roots, domain events
- `generate` command outputs C# entity classes with RVR conventions
- `diagram` command outputs Mermaid class diagrams
- Supports OpenAI, Claude, Ollama backends

### Added - RVR Studio Visual Domain Modeling
- Blazor page `/domain-modeling` for visual bounded context design
- Entity/ValueObject/DomainEvent/Relationship editors
- Export to JSON, Mermaid, PlantUML, C# code
- Code generator follows RVR.Framework conventions (AggregateRoot, EF configs, DTOs, repos)

### Added - Documentation Portal (VitePress)
- Bilingual FR/EN documentation site
- Getting started, architecture, security, multi-tenancy guides
- Module documentation (Core, Security, Caching, Jobs, Export, Webhooks, GraphQL, Billing)
- CLI command reference
- GitHub Pages deployment workflow

### Fixed - Security (GitHub Code Scan)
- AuthService: sanitize user input in log messages (prevent log injection)
- GlobalExceptionHandler: never expose exception details in production
- CI/deploy workflows: explicit permissions blocks

---

## [3.0.0] - 2026-03-16

### Upgrade .NET 9
- **Runtime** : .NET 8.0 -> .NET 9.0 (42 projets)
- **Langage** : C# 12 -> C# 13 (LangVersion 13.0)
- **Packages** : 31 packages Microsoft mis a jour vers 9.0.0
- **Serilog** : 3.1.1 -> 4.2.0, **OpenAI SDK** : beta -> 2.2.0 stable

### Added - 7 nouveaux modules

#### RVR.Framework.Export
- Export PDF (QuestPDF), Excel (ClosedXML), CSV
- Interface `IExportService` avec `ExportOptions` configurable

#### RVR.Framework.Webhooks
- Systeme publish/subscribe avec HMAC-SHA256 signing
- Retry avec backoff exponentiel, `InMemoryWebhookStore`

#### RVR.Framework.GraphQL
- HotChocolate 14.3 avec Query, Mutation
- Filtering, sorting, projection automatiques

#### RVR.Framework.Client
- Client API type C# (`RvrApiClient`)
- Auth, Products, Users, Health endpoints

#### RVR.Framework.Admin
- Dashboard Blazor Server (5 pages)
- Dashboard stats, Users, Products, Audit Logs, Health

#### RVR.Framework.AI
- Module RAG (Retrieval-Augmented Generation)
- `IChatClient`, `IEmbeddingService`, `IVectorStore` abstractions
- `InMemoryVectorStore` avec cosine similarity SIMD
- `SlidingWindowChunker` pour ingestion documentaire

#### RVR.Framework.NaturalQuery
- Natural Language Query Builder (FR/EN)
- Parseur avec fuzzy matching Levenshtein
- Expression builder LINQ dynamique
- "produits actifs prix > 100" -> `Where(p => p.IsActive && p.Price > 100)`

### Added - Features de securite et performance

- **F1 Refresh Token Storage** : Persistance EF Core avec rotation et revocation
- **F2 Trusted Proxy** : Configuration `TrustedProxies` pour rate limiting
- **F4 Account Lockout** : Anti brute-force (5 tentatives, 15 min, configurable)
- **F5 ETag Caching** : Middleware SHA256, 304 Not Modified
- **F6 Compiled Queries** : Centralisees dans `CompiledQueries.cs`
- **F13 Docker Compose** : 8 services dev (SQL, PG, Redis, RabbitMQ, Prometheus, Grafana, Jaeger, Seq)
- **F14 Testcontainers** : Integration tests avec MsSql + Redis reels

### Added - CLI AI Review avance

- **Architecture Analyzer** : Validation Clean Architecture layer dependencies (ARCH001-ARCH005)
- **DDD Analyzer** : Detection anti-patterns DDD (DDD001-DDD008)
- **Performance Analyzer** : N+1 queries, missing async, EF Core anti-patterns (PERF001-PERF010)
- **Security Analyzer** : SQL injection, XSS, hardcoded secrets (SEC001-SEC012)
- **LLM Backends** : OpenAI, Claude, Ollama (offline)
- **Output Formats** : Console (Spectre.Console), JSON, SARIF 2.1.0 (CI)
- Commande : `rvr ai review --architecture --security --output sarif`

### Fixed - 15 anomalies corrigees

- `AuthService` : `NotImplementedException` -> implementation complete
- `RVRDbContext` : Base64 -> AES-256 encryption
- CORS : `AllowAnyOrigin` -> restrictif en production
- JWT : `RequireHttpsMetadata` conditionnel
- HealthChecks : packages dupliques supprimes
- CI/CD : format check et security audit echouent le build
- 22 projets manquants ajoutes au .sln
- `FeatureManagement` : module implemente (etait TODO vide)
- `ResultMappingMiddleware` : dead code supprime

### Removed

- `.auto-claude-security.json`, `.auto-claude-status`, `.claude_settings.json` du repo
- `ResultMappingMiddleware` (dead code)
- Packages NuGet fictifs (AWS HealthChecks inexistants)

---

## [2.3.0] - 2026-03-16

### 🚀 Native AOT & Performance

#### Changed
- **AutoMapper → Mapperly Migration** : Remplacement complet d'AutoMapper par Mapperly source generators.
  - **Compile-Time Mapping** : Toutes les conversions Entity ↔ DTO sont générées à la compilation, éliminant la réflexion runtime.
  - **Native AOT Ready** : Le framework est désormais compatible avec la compilation AOT native .NET, permettant des déploiements optimisés avec un temps de démarrage réduit.
  - **Performance Améliorée** : Suppression de l'overhead de réflexion d'AutoMapper, réduisant l'empreinte mémoire et accélérant le démarrage de l'application.
  - **Compile-Time Safety** : Les erreurs de mapping sont détectées lors du build, pas en production.
  - **Mappers Créés** :
    - `TenantMapper` (Tenant ↔ TenantDto)
    - `UserMapper` (User ↔ UserDto)
    - `ProductMapper` (Product ↔ ProductDto)
    - `OrderMapper` (Order ↔ OrderDto)
    - `FeatureFlagMapper` (FeatureFlag ↔ FeatureFlagDto)
    - `AuditLogMapper` (AuditLog ↔ AuditLogDto)
  - **Architecture Pattern** : Tous les mappers utilisent `[Mapper] partial class` avec des méthodes partielles pour un mapping bidirectionnel.

#### Removed
- **AutoMapper** : Suppression complète du package AutoMapper et de ses dépendances (v12.0.1).
  - Plus de réflexion runtime pour le mapping d'objets
  - Déblocage du chemin de compilation Native AOT
  - Avantage compétitif sur ABP Framework et Clean Architecture templates qui utilisent encore AutoMapper

---

## [2.2.0] - 2026-03-13

### 💎 Enterprise Edition (Gratuite) & Performance Cloud Native

#### Added
- **.NET Aspire Support** (`RVR.Framework.AppHost`) : Orchestration complète, monitoring centralisé et service discovery natif.
- **Outbox Pattern** : Publication fiable d'événements de domaine via la table `OutboxMessages` et un job Quartz asynchrone.
- **SignalR Real-Time** (`RVR.Framework.RealTime`) : Hub multi-tenant pour notifications temps réel avec groupes automatiques par TenantId et UserId.
- **Specification Pattern** : Abstraction de requêtes complexes (`ISpecification`) réutilisables et testables isolément.
- **Module Boundaries** (`IRvrModule`) : Architecture monolithique modulaire avec découverte automatique des modules via scan d'assemblies.
- **Performance & Scalabilité** :
  - **EF Core Compiled Queries** : Réduction drastique de l'overhead de compilation LINQ.
  - **DbContext Pooling** : Réutilisation des instances de contexte pour une meilleure montée en charge.
  - **Bulk Operations** : Intégration de `EFCore.BulkExtensions` pour les traitements de masse performants.
  - **Output Caching** : Support du nouveau middleware .NET 8/9 pour le cache de réponse granulaire.
  - **Response Compression** : Compression Brotli/Gzip activée par défaut pour réduire la latence.
- **Security & Reliability** :
  - **API Key Management** : Authentification par clé `X-API-KEY` avec gestion du cycle de vie (expiration, audit).
  - **Secret Rotation** : Service `ISecretRotationManager` pour la rotation automatique des clés (JWT, etc.).
  - **Data Encryption at Rest** : Attribut `[EncryptedAtRest]` pour le chiffrement transparent des colonnes sensibles.
  - **Soft Delete amélioré** : Suppression logique automatisée avec filtre de requête global.
  - **Security Headers (OWASP)** : Middleware configuré pour injecter CSP, HSTS, X-Frame-Options, etc.
  - **Advanced Health Checks** : Monitoring profond (DB, Redis, API) avec réponses JSON détaillées (UI Client).
- **Email & Notifications** (`RVR.Framework.Notifications`) : Provider SMTP prêt à l'emploi et abstraction pour intégrations futures (SendGrid, Twilio).
- **Minimal API Discovery** : Mapping automatique des endpoints implémentant `IMapEndpoints`.

#### Enterprise Features (Studio Redesign)
- **Local & Cloud AI Support** : Intégration complète de vrais SDKs pour OpenAI, Anthropic (Claude), et LLM locaux (Ollama).
- **AI Generative UI** : Fonctionnalité "Baguette Magique" pour générer des schémas de base de données via prompt.
- **SaaS Billing & Stripe** : Gestion des abonnements, webhooks et MRR.
- **User Impersonation** : Support technique via connexion "en tant que".
- **Studio Redesign** : Nouveau thème "Premium SaaS" et dashboards interactifs.

---

## [2.1.0] - 2026-03-12

### 🛠️ Developer Experience (RVR Studio)

#### Added
- **Visual Entity Builder (Full-Stack)** - Nouvel outil visuel dans RVR Studio pour générer des entités complètes.
  - **Génération Physique** - Création automatique des fichiers `.cs` directement dans les dossiers de la solution (`src/`).
  - **Domain Layer** - Génération de l'entité (AggregateRoot/Entity) avec support Multitenant et Audit.
  - **Infrastructure Layer** - Génération de la configuration EF Core Fluent API.
  - **Application Layer** - Génération des DTOs (Request/Response) dans des dossiers pluralisés.
  - **API Layer** - Génération du Controller REST complet avec injection de service.
  - **Pluralisation automatique** - Gestion intelligente des noms de dossiers et de contrôleurs.

---

## [2.0.0] - 2025-03-08

### 🎉 Wave 1 - Documentation & Foundation

#### Added

##### Documentation
- **README.md** - Nouveau README avec badges, architecture ASCII, quickstart 5 minutes
- **docs/quickstart.md** - Guide de démarrage rapide avec installation, premier endpoint et tests
- **CONTRIBUTING.md** - Guide complet de contribution (code style, PR process, conventions)
- **CHANGELOG.md** - Historique des versions du framework
- **docs/plans/wave1-summary.md** - Résumé de la Wave 1 pour PR

##### Core Architecture
- **Clean Architecture** - Séparation stricte Domain/Application/Infrastructure/Api
- **Domain-Driven Design (DDD)** - Entités riches, value objects, domain events
- **Repository Pattern** - Abstraction complète de la couche de données
- **Dependency Injection** - Injection native .NET 8 avec lifetimes configurés

##### Multi-Tenancy & Security
- **Multi-Tenancy complet** - Isolation des données par tenant (TenantId)
- **JWT Authentication** - Tokens HMAC-SHA256 avec refresh tokens
- **Authorization** - Système de rôles, permissions et claims personnalisées
- **Audit Logging** - Traçabilité automatique de toutes les opérations
- **Global Error Handling** - Middleware d'exceptions centralisé avec logging structuré

##### Data & Performance
- **Entity Framework Core 8** - ORM moderne avec configurations fluent
- **Optimisations EF** :
  - `AsNoTracking()` pour toutes les requêtes en lecture
  - Split queries pour éviter les cartesian explosions
  - Retry logic pour résilience aux erreurs transitoires
  - Connection pooling optimisé
- **SQL Server** - Support natif avec migrations EF Core
- **Configurations EF structurées** - Section `EntityFrameworkSettings` dédiée

##### Validation & Quality
- **FluentValidation** - Validation robuste avec règles métier
  - Validators pour Products, Users, Auth
  - Règles de mot de passe fort (8+ caractères, majuscule, minuscule, chiffre, spécial)
  - Validation conditionnelle et messages personnalisés
- **Response standardization** - Format de réponse cohérent (`ErrorResponse`, `SuccessResponse`)
- **Development mode** - Détails complets des erreurs en environnement de développement

##### Developer Experience
- **Swagger/OpenAPI** - Documentation interactive avec tous les endpoints
- **ReDoc** - Documentation élégante en lecture seule
- **API Explorer** - Interface de test moderne avec authentification JWT intégrée
- **Serilog** - Logging structuré avec rotation de fichiers (30 jours de rétention)
- **Configuration environments** - Dev, staging, production avec variables d'environnement

##### Testing
- **xUnit** - Framework de tests unitaires
- **Moq** - Bibliothèque de mocking
- **Integration tests** - Tests d'intégration complets avec base de données InMemory
- **Test helpers** - Utilities pour tests isolés
- **Couverture de code** - Support coverlet pour rapports de couverture

##### DevOps & Deployment
- **Docker** - Containerisation prête (Dockerfile, docker-compose.yml)
- **IIS Deployment** - Script PowerShell automatisé (`deploy-iis.ps1`)
- **Health checks** - Endpoints de monitoring
- **Helm charts** - Déploiement Kubernetes (`helm/RIVORA-framework/`)
- **GitHub Actions** - Workflows CI/CD (`.github/workflows/`)

##### Monitoring & Observability
- **Prometheus** - Configuration des métriques (`ops/prometheus-alerts.yml`)
- **Loki** - Logging centralisé (`ops/loki-config.yml`)
- **OpenTelemetry** - Collector configuration (`ops/otel-collector-config.yml`)

---

### 🚀 Wave 2 - Advanced Features

#### Added

##### Feature Flags (`RVR.Framework.Features`)
- **Config Provider** - Features depuis appsettings.json
- **Database Provider** - Features stockées en base de données
- **Azure Provider** - Azure App Configuration integration
- **Feature Gates** - Attribute `[FeatureGate]` pour controllers
- **Feature Dashboard** - UI Razor Pages pour gestion des features
- **Multiple Providers** - Support de plusieurs providers simultanés

##### Caching (`RVR.Framework.Caching`)
- **Memory Cache** - Cache en mémoire locale
- **Redis Cache** - Cache distribué avec StackExchange.Redis
- **Response Caching** - Middleware de cache HTTP
- **Tag Invalidation** - Invalidation par tags
- **Serialization** - MessagePack pour performance
- **Cache Helper** - Utilities pour génération de keys

##### API Versioning (`RVR.Framework.ApiVersioning`)
- **URL Path** - Versioning par URL (`/v1/products`)
- **Header** - Versioning par header (`X-API-Version: 1`)
- **Query String** - Versioning par query param (`?api-version=1`)
- **Media Type** - Versioning par Accept header
- **Swagger Integration** - Documentation par version
- **Deprecation** - Support des versions dépréciées

##### Extended Health Checks (`RVR.Framework.HealthChecks`)
- **80+ Health Checks** - Database, Redis, RabbitMQ, AI providers, etc.
- **Multiple Endpoints** - `/health`, `/health/ready`, `/health/detailed`
- **Health UI** - Dashboard de monitoring
- **Custom Writers** - JSON, Prometheus, custom formats
- **Alerting** - Integration Prometheus, Datadog, Seq, Application Insights

---

### ⚙️ Wave 3 - Jobs & Background Processing

#### Added

##### Jobs Abstractions (`RVR.Framework.Jobs.Abstractions`)
- **IJobScheduler** - Interface unifiée pour scheduling
- **Job Models** - Modèles communs pour jobs
- **Job Options** - Configuration centralisée

##### Hangfire Integration (`RVR.Framework.Jobs.Hangfire`)
- **Hangfire Core** - Integration avec Hangfire
- **Dashboard** - Dashboard Hangfire avec auth
- **SQL Server Storage** - Persistence SQL Server
- **Redis Storage** - Persistence Redis
- **Retry Strategy** - Retry automatique avec configuration
- **Job Filters** - Logging et monitoring filters

##### Quartz.NET Integration (`RVR.Framework.Jobs.Quartz`)
- **Quartz Core** - Integration avec Quartz.NET
- **Cron Scheduling** - Expressions cron avancées
- **Persistence** - Database persistence optionnelle
- **Clustering** - Support clustering Quartz
- **Plugins** - Time zone converter plugin

---

### 🔒 Wave 4 - Security Enhancements

#### Added

##### 2FA/TOTP (`RVR.Framework.Security`)
- **TOTP Service** - Time-based One-Time Password
- **QR Code Generation** - Setup 2FA avec QRCoder
- **Backup Codes** - Codes de récupération
- **2FA Verification** - Validation des codes TOTP
- **2FA Enforcement** - Middleware d'obligation 2FA

##### RBAC & Permissions (`RVR.Framework.Security`)
- **Permission Service** - Gestion des permissions
- **RequirePermission Attribute** - `[RequirePermission("Products.View")]`
- **Hierarchical Permissions** - Permissions imbriquées
- **Permission Store** - In-memory et database stores
- **Authorization Handler** - PermissionAuthorizationHandler

##### Rate Limiting (`RVR.Framework.Security`)
- **Rate Limit Service** - Service de limitation de débit
- **Rate Limit Attribute** - `[RateLimit(10, 60)]`
- **In-Memory Store** - Store en mémoire
- **Redis Store** - Store distribué Redis
- **Configurable Rules** - Règles par endpoint

##### Audit Trail (`RVR.Framework.Security`)
- **Audit Service** - Service d'audit
- **Audit Interceptor** - EF Core interceptor
- **Audit Store** - In-memory et database stores
- **Audit Entry** - Modèle d'entrée d'audit
- **Audit Query** - Recherche dans les logs

##### Refresh Tokens (`RVR.Framework.Security`)
- **Refresh Token Service** - Gestion des refresh tokens
- **Token Cleanup Job** - Nettoyage automatique
- **Sliding Expiration** - Expiration glissante
- **Token Revocation** - Révocation des tokens

---

### 🤖 Wave 5 - AI & CLI

#### Added

##### RVR.CLI (`tools/RVR.CLI`)
- **rvr new** - Création de projets avec templates
  - Template `minimal` - Projet de base
  - Template `saas-starter` - SaaS avec auth
  - Template `ai-rag` - Projet avec AI RAG
  - Options `--tenancy` (row, schema, database)

- **rvr generate** - Génération de code
  - `aggregate` - Aggregate root complet
  - `crud` - CRUD operations
  - `command` - CQRS command
  - `query` - CQRS query
  - Aliases: `gen`, `g`

- **rvr ai** - Commandes AI
  - `ai chat` - Chat interactif OpenAI/Claude
  - `ai generate` - Génération de code avec AI
  - `ai review` - Review de code avec AI
  - Support OpenAI (gpt-4o, gpt-3.5-turbo)
  - Support Claude (claude-3-5-sonnet, etc.)

- **rvr benchmark** - Load testing avec k6
  - Scénarios: smoke, load, stress, spike, soak
  - Options: duration, vus, output format
  - Integration CI/CD

- **rvr doctor** - Diagnostic de projet
  - Checks: SDK, restore, build, tests, migrations
  - Health score calculation
  - Recommendations automatiques

- **rvr add-module** - Ajout de modules complets
- **rvr dev** - Serveur de développement
- **rvr migrate** - Migrations database
- **rvr seed** - Seed database
- **rvr completion** - Shell completion scripts

---

### 📦 Database Providers

#### Added

##### SqlServer (`RVR.Framework.Data.SqlServer`)
- **SqlServerDbContext** - DbContext base pour SQL Server
- **SqlServerDbProvider** - Provider configuration
- **Retry Logic** - Retry on failures
- **Migrations** - EF Core migrations support

##### PostgreSQL (`RVR.Framework.Data.PostgreSQL`)
- **PostgreSQLDbContext** - DbContext base pour PostgreSQL
- **PostgreSQLDbProvider** - Provider configuration
- **Npgsql** - Npgsql provider
- **Migrations** - EF Core migrations support

##### MySQL (`RVR.Framework.Data.MySQL`)
- **MySqlDbContext** - DbContext base pour MySQL
- **MySqlDbProvider** - Provider configuration
- **Pomelo** - Pomelo.EntityFrameworkCore.MySql
- **Migrations** - EF Core migrations support

##### SQLite (`RVR.Framework.Data.SQLite`)
- **SqliteDbContext** - DbContext base pour SQLite
- **SqliteDbProvider** - Provider configuration
- **Development** - Ideal for dev/testing
- **Migrations** - EF Core migrations support

---

### 📚 Documentation Updates

#### Added

##### Module Documentation (`docs/modules/`)
- **database.md** - Database providers documentation
- **jobs.md** - Jobs & background processing
- **features.md** - Feature flags documentation
- **security.md** - Security features (2FA, RBAC, RateLimiting, Audit)
- **health-checks.md** - Health checks documentation
- **caching.md** - Caching documentation
- **api-versioning.md** - API versioning documentation
- **ai-native.md** - AI features documentation

##### CLI Documentation (`docs/cli/`)
- **rvr-new.md** - rvr new command documentation
- **rvr-generate.md** - rvr generate command documentation
- **rvr-doctor.md** - rvr doctor command documentation
- **rvr-ai.md** - rvr ai commands documentation
- **rvr-benchmark.md** - rvr benchmark command documentation

---

### Changed

##### Architecture
- Migration vers .NET 8
- Restructuration complète en Clean Architecture
- Séparation stricte des responsabilités entre couches
- Introduction des AggregateRoots et Value Objects

##### Configuration
- Configuration SQL structurée dans `appsettings.json`
- Section `DatabaseSettings` pour paramètres EF Core
- Section `JwtSettings` pour authentification
- Section `Serilog` pour logging

##### Code Quality
- Adoption des Conventional Commits
- Standards de code style C# Microsoft
- Commentaires XML pour API publiques
- Tests obligatoires pour nouvelles features

---

### Deprecated

- Ancienne architecture monolithique (remplacée par Clean Architecture)
- Configuration inline dans Program.cs (remplacée par sections structurées)
- Logging Console.WriteLine (remplacé par Serilog)

---

### Removed

- Dépendances non utilisées
- Code redondant entre couches
- Configurations hardcodées

---

### Fixed

- Problèmes de isolation multi-tenancy
- Fuites de mémoire dans DbContext
- Exceptions non gérées dans les controllers
- Validation incohérente des DTOs

---

### Security

- **JWT Secret Key** - Configuration externalisée
- **HTTPS** - Support natif avec redirection
- **CORS** - Configuration sécurisée
- **SQL Injection** - Protection via EF Core parameterized queries
- **XSS** - Encodage automatique des sorties
- **Rate Limiting** - Protection contre les abus
- **2FA** - Authentication à deux facteurs
- **Audit Trail** - Traçabilité des opérations

---

## [1.0.0] - 2024-10-15

### Initial Release

#### Added

- Structure de base du framework
- Entités Identity (Users, Roles, UserRoles)
- Multi-Tenancy basique
- Repository Pattern implémentation
- CRUD Products exemple
- Scripts de déploiement IIS
- Documentation initiale (GUIDE-COMPLET.md, README.md)

---

## Versions

| Version | Date | Description |
|---------|------|-------------|
| [3.2.0](#320---2026-03-17) | 2026-03-17 | OAuth2/OIDC, GDPR, Event Sourcing, Saga, Keyset pagination, 14 features |
| [3.1.0](#310---2026-03-16) | 2026-03-16 | Billing, Benchmarks, AI Design, Studio Modeling, Doc Portal |
| [3.0.0](#300---2026-03-16) | 2026-03-16 | .NET 9 upgrade, 7 new modules, CLI AI Review, 19 features |
| [2.3.0](#230---2026-03-16) | 2026-03-16 | Mapperly migration, Native AOT ready |
| [2.2.0](#220---2026-03-13) | 2026-03-13 | Performance Cloud Native, .NET Aspire, Outbox & Specification Patterns |
| [2.1.0](#210---2026-03-12) | 2026-03-12 | RVR Studio - Visual Entity Builder |
| [2.0.0](#200---2025-03-08) | 2025-03-08 | Waves 1-5 - Foundation, Features, Jobs, Security, AI/CLI |
| [1.0.0](#100---2024-10-15) | 2024-10-15 | Initial Release |

---

## Notes

### Conventions de versionnage

- **MAJOR** (2.0.0) - Changements incompatibles (breaking changes)
- **MINOR** (2.1.0) - Nouvelles fonctionnalités rétrocompatibles
- **PATCH** (2.0.1) - Corrections de bugs rétrocompatibles

### Waves

Le développement suit un modèle de "Waves" :

| Wave | Focus | Status |
|------|-------|--------|
| Wave 1 | Documentation & Foundation | Complet |
| Wave 2 | Advanced Features | Complet |
| Wave 3 | Jobs & Background Processing | Complet |
| Wave 4 | Security Enhancements | Complet |
| Wave 5 | AI & CLI | Complet |
| Wave 6 | Cloud Native & Reliability | Complet |
| Wave 7 | .NET 9, Export, Webhooks, GraphQL, Admin, AI/RAG, NL Query | Complet |

---

**RIVORA Framework Team** - [khalilbenaz/RIVORA](https://github.com/khalilbenaz/RIVORA)
