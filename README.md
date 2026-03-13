# KBA Framework

![.NET 8/9](https://img.shields.io/badge/.NET-8.0%20%2F%209.0-512BD4?logo=dotnet&style=flat-square)
![License](https://img.shields.io/badge/license-MIT-green?style=flat-square)
![Build](https://img.shields.io/badge/build-passing-brightgreen?style=flat-square)
![Coverage](https://img.shields.io/badge/coverage-85%25-success?style=flat-square)
![Version](https://img.shields.io/badge/version-2.2.0-blue?style=flat-square)

**Framework d'entreprise Cloud-Native basé sur Clean Architecture, DDD et Multi-tenancy pour applications SaaS professionnelles.**

✨ **Nouveau dans la v2.2.0 : Performance & Reliability.** Intégration de .NET Aspire pour l'orchestration, Outbox Pattern pour des événements fiables, gRPC haute performance, et optimisations EF Core majeures (Pooling, Compiled Queries).

---

## 📋 Table des Matières

- [🚀 Démarrage Rapide](#-démarrage-rapide)
- [🏗️ Architecture](#-architecture)
- [✨ Features (Waves 1-6)](#-features-waves-1-6)
- [🛠️ KBA Studio & CLI](#-kba-studio--cli)
- [📦 Modules & Packages](#-modules--packages)
- [📚 Documentation](#-documentation)
- [🤝 Contributing](#-contributing)

---

## 🚀 Démarrage Rapide

### Prérequis
- [.NET 8 ou 9 SDK](https://dotnet.microsoft.com/download)
- SQL Server, PostgreSQL, MySQL ou SQLite
- Git
- (Optionnel) .NET Aspire Workload pour l'orchestration locale

### Installation Rapide
```bash
# 1. Cloner le repository
git clone https://github.com/khalilbenaz/KBA.Framework.git
cd KBA.Framework

# 2. Restaurer et Lancer
dotnet restore
dotnet run --project src/KBA.Framework.Api
```

📖 **Guide complet d'installation** → [docs/quickstart.md](docs/quickstart.md)

---

## 🏗️ Architecture

### Clean Architecture Modernisée
Le framework suit les principes de la Clean Architecture tout en intégrant des patterns de fiabilité modernes :

- **Presentation (API)** : Controllers, Minimal APIs (Auto-discovery), gRPC, SignalR Real-Time.
- **Application** : Services, DTOs, FluentValidation, MediatR, Result Pattern (ROP).
- **Infrastructure** : EF Core 8/9, DbContext Pooling, Compiled Queries, Outbox Processor, Redis Caching.
- **Domain** : Entités riches, Value Objects, Domain Events, Specifications, ISoftDelete.

---

## ✨ Features Waves 1-6

### 🌊 Wave 1 - Documentation & Foundation
| Feature | Description | Status |
|---------|-------------|--------|
| **Clean Architecture** | Séparation stricte Domain/Application/Infrastructure/Api | ✅ |
| **Domain-Driven Design** | Entités riches, value objects, domain events, aggregates | ✅ |
| **Repository Pattern** | Abstraction complète de la couche de données | ✅ |
| **Dependency Injection** | Injection native .NET 8 avec lifetimes configurés | ✅ |
| **Multi-Tenancy** | Isolation des données par tenant (TenantId) | ✅ |
| **JWT Authentication** | Tokens HMAC-SHA256 avec refresh tokens | ✅ |
| **Authorization** | Rôles, permissions, claims personnalisées | ✅ |
| **Audit Logging** | Traçabilité automatique de toutes les opérations | ✅ |
| **Entity Framework Core 8** | ORM moderne avec configurations fluent | ✅ |
| **FluentValidation** | Validation robuste avec règles métier | ✅ |
| **Serilog** | Logging structuré avec rotation de fichiers | ✅ |
| **Swagger/OpenAPI** | Documentation interactive | ✅ |
| **Docker** | Containerisation prête | ✅ |
| **Health Checks** | Endpoints de monitoring | ✅ |

### 🌊 Wave 2 - Advanced Features
| Feature | Description | Status |
|---------|-------------|--------|
| **Feature Flags** | Config, Database, Azure App Configuration providers | ✅ |
| **Feature Gates** | Attribute-based feature toggling | ✅ |
| **Feature Dashboard** | UI de gestion des features | ✅ |
| **Caching** | Memory et Redis avec tags invalidation | ✅ |
| **Response Caching** | Middleware de cache HTTP | ✅ |
| **API Versioning** | URL path, header, query string, media type | ✅ |
| **Health Checks Extended** | 80+ checks (DB, Redis, RabbitMQ, AI, etc.) | ✅ |
| **Health UI** | Dashboard de monitoring | ✅ |

### 🌊 Wave 3 - Jobs & Background Processing
| Feature | Description | Status |
|---------|-------------|--------|
| **Hangfire Integration** | Jobs background avec dashboard | ✅ |
| **Quartz.NET Integration** | Scheduling avancé avec cron | ✅ |
| **Job Abstractions** | Interface commune IJobScheduler | ✅ |
| **Job Monitoring** | Health checks pour jobs | ✅ |
| **Retry Strategy** | Retry automatique avec backoff | ✅ |
| **Job Scheduling** | API unifiée pour scheduler jobs | ✅ |

### 🌊 Wave 4 - Security Enhancements
| Feature | Description | Status |
|---------|-------------|--------|
| **2FA/TOTP** | Authentication à deux facteurs | ✅ |
| **QR Code Generation** | Setup 2FA avec QR codes | ✅ |
| **Backup Codes** | Codes de récupération 2FA | ✅ |
| **RBAC** | Role-Based Access Control hiérarchique | ✅ |
| **Permission System** | Permissions granulaires | ✅ |
| **Rate Limiting** | Limitation de débit configurable | ✅ |
| **Audit Trail** | Interceptor EF Core pour audit | ✅ |
| **Refresh Token Cleanup** | Nettoyage automatique tokens | ✅ |

### 🌊 Wave 5 - AI & CLI
| Feature | Description | Status |
|---------|-------------|--------|
| **KBA.CLI** | CLI de scaffolding et génération | ✅ |
| **kba new** | Création de projets templates | ✅ |
| **kba generate** | Génération de code (aggregate, CRUD, CQRS) | ✅ |
| **kba ai chat** | Chat interactif avec LLM (OpenAI/Claude/Ollama/Kilo) | ✅ |
| **kba ai generate** | Génération de code avec AI | ✅ |
| **kba ai review** | Review de code avec AI | ✅ |
| **kba benchmark** | Load testing avec k6 | ✅ |
| **kba doctor** | Diagnostic de projet | ✅ |
| **kba add-module** | Ajout de modules complets | ✅ |

### 🌊 Wave 6 - Cloud Native & Reliability (v2.2.0)
| Feature | Description | Status |
|---------|-------------|--------|
| **.NET Aspire** | Orchestration, service discovery et dashboard de monitoring | ✅ |
| **Outbox Pattern** | Publication fiable d'événements via table outbox et Quartz | ✅ |
| **Specification Pattern** | Abstraction de requêtes complexes et réutilisables | ✅ |
| **gRPC Support** | Endpoints RPC haute performance pour com-inter-services | ✅ |
| **Compiled Queries** | Optimisation EF Core pour réduire l'overhead LINQ | ✅ |
| **DbContext Pooling** | Performance accrue par réutilisation des contextes | ✅ |
| **Bulk Operations** | Traitements de masse (Insert/Update/Delete) ultra-rapides | ✅ |
| **SignalR Real-Time** | Hub multi-tenant pour notifications temps réel | ✅ |
| **API Keys** | Authentification par clé X-API-KEY avec cycle de vie | ✅ |
| **Secret Rotation** | Gestion dynamique et sécurisée des secrets applicatifs | ✅ |
| **Soft Delete** | Suppression logique automatisée avec filtres globaux | ✅ |
| **Module Boundaries** | Architecture monolithique modulaire (IKbaModule) | ✅ |

---

## 🛠️ KBA Studio & CLI

### Visual Entity Builder
Concevez vos entités graphiquement dans **KBA Studio** et laissez l'IA générer le code source physique directement dans votre dossier `src/`.

### CLI Commandes
```bash
# Créer un projet SaaS
kba new MySaaS --template saas-starter

# Générer un CRUD complet
kba generate crud Invoice --props "Reference:string,Amount:decimal"

# Diagnostic du système
kba doctor
```

---

## 📦 Modules & Packages

| Module | Namespace | Usage |
|--------|-----------|-------|
| **Core** | `KBA.Framework.Core` | Interfaces de base, Modules, Specifications |
| **Real-Time** | `KBA.Framework.RealTime` | SignalR Hubs multi-tenant |
| **Notifications** | `KBA.Framework.Notifications` | Email (SMTP), Push, SMS |
| **Security** | `KBA.Framework.Security` | Encryption, Key Rotation, Api Keys |
| **Aspire** | `KBA.Framework.AppHost` | Orchestration Cloud-Native |

---

## 📚 Documentation

| Guide | Contenu |
|-------|---------|
| [Quickstart](docs/quickstart.md) | Installation en 5 minutes |
| [Complet](docs/GUIDE-COMPLET.md) | Architecture et Patterns détaillés |
| [Modules](docs/modules/) | Documentation spécifique par composant |
| [CLI](docs/cli/) | Référence toutes les commandes du CLI |

---

## 🤝 Contributing

Les contributions sont les bienvenues ! Consultez [CONTRIBUTING.md](CONTRIBUTING.md) pour les détails sur le workflow de PR et les standards de code.

---

## 📄 License

Distribué sous la licence **MIT**. Voir `LICENSE` pour plus d'informations.

---

**KBA Framework Team** - *Production-Ready Architecture for Modern .NET Applications*
