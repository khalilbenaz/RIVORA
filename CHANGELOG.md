# Changelog

Tous les changements notables de KBA Framework sont documentés dans ce fichier.

Le format est basé sur [Keep a Changelog](https://keepachangelog.com/fr/1.0.0/),
et ce projet adhère au [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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

#### Changed

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

#### Deprecated

- Ancienne architecture monolithique (remplacée par Clean Architecture)
- Configuration inline dans Program.cs (remplacée par sections structurées)
- Logging Console.WriteLine (remplacé par Serilog)

#### Removed

- Dépendances non utilisées
- Code redondant entre couches
- Configurations hardcodées

#### Fixed

- Problèmes de isolation multi-tenancy
- Fuites de mémoire dans DbContext
- Exceptions non gérées dans les controllers
- Validation incohérente des DTOs

#### Security

- **JWT Secret Key** - Configuration externalisée
- **HTTPS** - Support natif avec redirection
- **CORS** - Configuration sécurisée
- **SQL Injection** - Protection via EF Core parameterized queries
- **XSS** - Encodage automatique des sorties

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
| [2.0.0](#200---2025-03-08) | 2025-03-08 | Wave 1 - Documentation & Foundation |
| [1.0.0](#100---2024-10-15) | 2024-10-15 | Initial Release |

---

## Notes

### Conventions de versionnage

- **MAJOR** (2.0.0) - Changements incompatibles (breaking changes)
- **MINOR** (2.1.0) - Nouvelles fonctionnalités rétrocompatibles
- **PATCH** (2.0.1) - Corrections de bugs rétrocompatibles

### Waves

Le développement suit un modèle de "Waves" :

| Wave | Focus | Statut |
|------|-------|--------|
| Wave 1 | Documentation & Foundation | ✅ Complet |
| Wave 2 | Features avancées | 🔄 En cours |
| Wave 3 | Performance & Scale | 📋 Planifié |

---

**KBA Framework Team** - [khalilbenaz/KBA.Framework](https://github.com/khalilbenaz/KBA.Framework)
