# Changelog

Tous les changements notables de KBA Framework sont documentés dans ce fichier.

Le format est basé sur [Keep a Changelog](https://keepachangelog.com/fr/1.0.0/),
et ce projet adhère au [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [2.1.0] - 2026-03-12

### 🛠️ Developer Experience (KBA Studio)

#### Added
- **Visual Entity Builder (Full-Stack)** - Nouvel outil visuel dans KBA Studio pour générer des entités complètes.
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
- **Helm charts** - Déploiement Kubernetes (`helm/kba-framework/`)
- **GitHub Actions** - Workflows CI/CD (`.github/workflows/`)

##### Monitoring & Observability
- **Prometheus** - Configuration des métriques (`ops/prometheus-alerts.yml`)
- **Loki** - Logging centralisé (`ops/loki-config.yml`)
- **OpenTelemetry** - Collector configuration (`ops/otel-collector-config.yml`)

---

### 🚀 Wave 2 - Advanced Features

#### Added

##### Feature Flags (`KBA.Framework.Features`)
- **Config Provider** - Features depuis appsettings.json
- **Database Provider** - Features stockées en base de données
- **Azure Provider** - Azure App Configuration integration
- **Feature Gates** - Attribute `[FeatureGate]` pour controllers
- **Feature Dashboard** - UI Razor Pages pour gestion des features
- **Multiple Providers** - Support de plusieurs providers simultanés

##### Caching (`KBA.Framework.Caching`)
- **Memory Cache** - Cache en mémoire locale
- **Redis Cache** - Cache distribué avec StackExchange.Redis
- **Response Caching** - Middleware de cache HTTP
- **Tag Invalidation** - Invalidation par tags
- **Serialization** - MessagePack pour performance
- **Cache Helper** - Utilities pour génération de keys

##### API Versioning (`KBA.Framework.ApiVersioning`)
- **URL Path** - Versioning par URL (`/v1/products`)
- **Header** - Versioning par header (`X-API-Version: 1`)
- **Query String** - Versioning par query param (`?api-version=1`)
- **Media Type** - Versioning par Accept header
- **Swagger Integration** - Documentation par version
- **Deprecation** - Support des versions dépréciées

##### Extended Health Checks (`KBA.Framework.HealthChecks`)
- **80+ Health Checks** - Database, Redis, RabbitMQ, AI providers, etc.
- **Multiple Endpoints** - `/health`, `/health/ready`, `/health/detailed`
- **Health UI** - Dashboard de monitoring
- **Custom Writers** - JSON, Prometheus, custom formats
- **Alerting** - Integration Prometheus, Datadog, Seq, Application Insights

---

### ⚙️ Wave 3 - Jobs & Background Processing

#### Added

##### Jobs Abstractions (`KBA.Framework.Jobs.Abstractions`)
- **IJobScheduler** - Interface unifiée pour scheduling
- **Job Models** - Modèles communs pour jobs
- **Job Options** - Configuration centralisée

##### Hangfire Integration (`KBA.Framework.Jobs.Hangfire`)
- **Hangfire Core** - Integration avec Hangfire
- **Dashboard** - Dashboard Hangfire avec auth
- **SQL Server Storage** - Persistence SQL Server
- **Redis Storage** - Persistence Redis
- **Retry Strategy** - Retry automatique avec configuration
- **Job Filters** - Logging et monitoring filters

##### Quartz.NET Integration (`KBA.Framework.Jobs.Quartz`)
- **Quartz Core** - Integration avec Quartz.NET
- **Cron Scheduling** - Expressions cron avancées
- **Persistence** - Database persistence optionnelle
- **Clustering** - Support clustering Quartz
- **Plugins** - Time zone converter plugin

---

### 🔒 Wave 4 - Security Enhancements

#### Added

##### 2FA/TOTP (`KBA.Framework.Security`)
- **TOTP Service** - Time-based One-Time Password
- **QR Code Generation** - Setup 2FA avec QRCoder
- **Backup Codes** - Codes de récupération
- **2FA Verification** - Validation des codes TOTP
- **2FA Enforcement** - Middleware d'obligation 2FA

##### RBAC & Permissions (`KBA.Framework.Security`)
- **Permission Service** - Gestion des permissions
- **RequirePermission Attribute** - `[RequirePermission("Products.View")]`
- **Hierarchical Permissions** - Permissions imbriquées
- **Permission Store** - In-memory et database stores
- **Authorization Handler** - PermissionAuthorizationHandler

##### Rate Limiting (`KBA.Framework.Security`)
- **Rate Limit Service** - Service de limitation de débit
- **Rate Limit Attribute** - `[RateLimit(10, 60)]`
- **In-Memory Store** - Store en mémoire
- **Redis Store** - Store distribué Redis
- **Configurable Rules** - Règles par endpoint

##### Audit Trail (`KBA.Framework.Security`)
- **Audit Service** - Service d'audit
- **Audit Interceptor** - EF Core interceptor
- **Audit Store** - In-memory et database stores
- **Audit Entry** - Modèle d'entrée d'audit
- **Audit Query** - Recherche dans les logs

##### Refresh Tokens (`KBA.Framework.Security`)
- **Refresh Token Service** - Gestion des refresh tokens
- **Token Cleanup Job** - Nettoyage automatique
- **Sliding Expiration** - Expiration glissante
- **Token Revocation** - Révocation des tokens

---

### 🤖 Wave 5 - AI & CLI

#### Added

##### KBA.CLI (`tools/KBA.CLI`)
- **kba new** - Création de projets avec templates
  - Template `minimal` - Projet de base
  - Template `saas-starter` - SaaS avec auth
  - Template `ai-rag` - Projet avec AI RAG
  - Options `--tenancy` (row, schema, database)

- **kba generate** - Génération de code
  - `aggregate` - Aggregate root complet
  - `crud` - CRUD operations
  - `command` - CQRS command
  - `query` - CQRS query
  - Aliases: `gen`, `g`

- **kba ai** - Commandes AI
  - `ai chat` - Chat interactif OpenAI/Claude
  - `ai generate` - Génération de code avec AI
  - `ai review` - Review de code avec AI
  - Support OpenAI (gpt-4o, gpt-3.5-turbo)
  - Support Claude (claude-3-5-sonnet, etc.)

- **kba benchmark** - Load testing avec k6
  - Scénarios: smoke, load, stress, spike, soak
  - Options: duration, vus, output format
  - Integration CI/CD

- **kba doctor** - Diagnostic de projet
  - Checks: SDK, restore, build, tests, migrations
  - Health score calculation
  - Recommendations automatiques

- **kba add-module** - Ajout de modules complets
- **kba dev** - Serveur de développement
- **kba migrate** - Migrations database
- **kba seed** - Seed database
- **kba completion** - Shell completion scripts

---

### 📦 Database Providers

#### Added

##### SqlServer (`KBA.Framework.Data.SqlServer`)
- **SqlServerDbContext** - DbContext base pour SQL Server
- **SqlServerDbProvider** - Provider configuration
- **Retry Logic** - Retry on failures
- **Migrations** - EF Core migrations support

##### PostgreSQL (`KBA.Framework.Data.PostgreSQL`)
- **PostgreSQLDbContext** - DbContext base pour PostgreSQL
- **PostgreSQLDbProvider** - Provider configuration
- **Npgsql** - Npgsql provider
- **Migrations** - EF Core migrations support

##### MySQL (`KBA.Framework.Data.MySQL`)
- **MySqlDbContext** - DbContext base pour MySQL
- **MySqlDbProvider** - Provider configuration
- **Pomelo** - Pomelo.EntityFrameworkCore.MySql
- **Migrations** - EF Core migrations support

##### SQLite (`KBA.Framework.Data.SQLite`)
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
- **kba-new.md** - kba new command documentation
- **kba-generate.md** - kba generate command documentation
- **kba-doctor.md** - kba doctor command documentation
- **kba-ai.md** - kba ai commands documentation
- **kba-benchmark.md** - kba benchmark command documentation

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
| [2.1.0](#210---2026-03-12) | 2026-03-12 | KBA Studio - Visual Entity Builder (Full-Stack) with physical file generation |
| [2.0.0](#200---2025-03-08) | 2025-03-08 | Waves 1-5 - Documentation, Features, Jobs, Security, AI/CLI |
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
| Wave 1 | Documentation & Foundation | ✅ Complet |
| Wave 2 | Advanced Features | ✅ Complet |
| Wave 3 | Jobs & Background Processing | ✅ Complet |
| Wave 4 | Security Enhancements | ✅ Complet |
| Wave 5 | AI & CLI | ✅ Complet |

---

**KBA Framework Team** - [khalilbenaz/KBA.Framework](https://github.com/khalilbenaz/KBA.Framework)
