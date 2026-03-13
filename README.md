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

### 🌊 Wave 6 - Cloud Native & Reliability (v2.2.0)
- **.NET Aspire** : Orchestration native, service discovery et dashboard de monitoring.
- **Outbox Pattern** : Publication fiable d'événements de domaine via Quartz.
- **Specification Pattern** : Abstraction de requêtes complexes et réutilisables.
- **gRPC Support** : Communication inter-services haute performance.
- **EF Core Optimisé** : Compiled Queries et DbContext Pooling pour une scalabilité accrue.
- **Security Headers** : Protection OWASP (CSP, HSTS, X-Frame-Options) activée par défaut.
- **API Key Auth** : Gestion du cycle de vie des clés `X-API-KEY`.

### 🌊 Wave 5 - AI & Developer Experience
- **KBA Studio** : Interface visuelle pour générer vos entités et votre code en un clic.
- **AI Generative UI** : Génération de schémas de base de données via prompts (OpenAI/Claude).
- **KBA.CLI** : Scaffolding ultra-rapide (`kba new`, `kba generate`).

### 🌊 Waves 1-4 - Fondations & Sécurité
- **Multi-Tenancy** : Isolation stricte des données (Row-level security).
- **Identity & Security** : Auth JWT, 2FA/TOTP, RBAC hiérarchique.
- **Audit Trail** : Traçabilité complète des modifications d'entités.
- **Background Jobs** : Intégration Hangfire et Quartz.NET.

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
