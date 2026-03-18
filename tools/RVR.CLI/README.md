# RVR CLI v4.0.0 - RIVORA Framework CLI

[![NuGet](https://img.shields.io/nuget/v/RVR.CLI.svg)](https://www.nuget.org/packages/RVR.CLI/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)

**CLI enterprise pour le framework RIVORA (.NET 9)** — scaffolding de projets, CRUD, modules, AI code review, domain design, migrations, environnements, publication et assistant de migration.

## Installation

```bash
dotnet tool install --global RVR.CLI
```

Mise a jour :

```bash
dotnet tool update --global RVR.CLI
```

## Commandes principales

### Scaffolding de projet

```bash
# Wizard interactif (recommande)
rvr new

# Nouveau projet SaaS multi-tenant
rvr new MyApp --template saas-starter

# API REST minimale
rvr new MyApi --template api-minimal

# Architecture microservices
rvr new MyPlatform --template microservices

# Application RAG avec vector store
rvr new MyAI --template ai-rag
```

### Generation de code

```bash
# CRUD complet (Entity + Command + Query + Controller + Tests)
rvr generate crud Invoice --props "Reference:string,Amount:decimal,DueDate:DateTime"

# Aggregate root DDD
rvr generate aggregate Order

# Client API type depuis OpenAPI
rvr generate client --url http://api/swagger/v1/swagger.json

# Tests unitaires scaffoldes
rvr generate test Product

# Data seeder scaffold
rvr generate seed Product --profile demo

# Module complet (Domain/Application/Infrastructure/API/Tests)
rvr add-module Inventory
```

### Module Management

```bash
# Ajouter un module
rvr add-module Inventory

# Retirer un module proprement
rvr remove-module Caching
rvr remove-module Caching --dry-run    # Previsualiser sans modifier
rvr remove-module Caching --force      # Ignorer les avertissements
```

### AI Review

Analyse automatique du code avec plusieurs analyzers :

```bash
# Tous les analyzers
rvr ai review --all

# Analyzers individuels
rvr ai review --architecture    # Clean Architecture conformance
rvr ai review --ddd             # DDD anti-patterns
rvr ai review --performance     # N+1, missing async, EF anti-patterns
rvr ai review --security        # Vulnerabilites OWASP

# Integration CI/CD (format SARIF 2.1.0)
rvr ai review --output sarif --output-file report.sarif

# Mode offline avec Ollama
rvr ai review --provider ollama
```

### AI Chat & Design

```bash
# Chat interactif avec LLM
rvr ai chat --provider claude

# Generation de code par prompt
rvr ai generate "Create a payment service with Stripe integration"

# Domain design interactif
rvr ai design --provider openai
```

### Database

```bash
# Migrations EF Core
rvr migrate generate AddOrderTable    # Generer migration
rvr migrate apply                     # Appliquer les migrations
rvr migrate list                      # Lister les migrations
rvr migrate rollback                  # Annuler derniere migration

# Seeding de donnees
rvr seed                              # Seed par defaut (profil dev)
rvr seed --profile demo               # Seed avec le profil demo
rvr seed --profile test --reset       # Truncate + reseed
rvr seed --dry-run                    # Previsualiser sans executer
rvr seed --tenant my-tenant           # Seed multi-tenant
```

### Environnements & Secrets

```bash
# Gestion des environnements
rvr env list                          # Lister les environnements
rvr env get ConnectionStrings:Default # Lire une variable
rvr env set DB:Host "localhost"       # Definir une variable
rvr env remove DB:Host               # Supprimer une variable
rvr env switch Production             # Changer d'environnement
rvr env diff Development Production   # Comparer deux envs

# Secrets (.NET User Secrets)
rvr env secrets init                  # Initialiser User Secrets
rvr env secrets set ApiKey "sk-..."   # Definir un secret

# Import/Export
rvr env export --format dotenv        # Exporter en .env
rvr env export --format json          # Exporter en JSON
rvr env export --format yaml          # Exporter en YAML
rvr env import --file .env            # Importer depuis .env
```

### Publication

```bash
# Publication unifiee (auto-detection)
rvr publish

# Cibles specifiques
rvr publish --target docker           # Image Docker (build + push)
rvr publish --target nuget            # Packages NuGet (pack + push)
rvr publish --target self-contained   # Binaires autonomes (win/linux/mac)
rvr publish --target azure            # Azure App Service

# Options
rvr publish --skip-tests              # Ignorer les tests
rvr publish --dry-run                 # Previsualiser les commandes
rvr publish --registry ghcr.io/user   # Registry custom
rvr publish --tag 2.0.0               # Version tag custom
```

### Upgrade (Migration entre versions)

```bash
# Lister les migrations disponibles
rvr upgrade --list

# Previsualiser la migration
rvr upgrade --to 4.0 --dry-run

# Appliquer la migration
rvr upgrade --to 4.0
```

### DevOps & Diagnostic

```bash
rvr doctor       # Diagnostic complet (SDK, DB, Redis, etc.)
rvr benchmark    # Load testing k6
rvr dev          # Serveur de developpement
```

## Templates disponibles

| Template | Description |
|----------|-------------|
| `saas-starter` | Application SaaS multi-tenant complete |
| `api-minimal` | API REST minimale avec Clean Architecture |
| `microservices` | Architecture microservices avec API gateway |
| `ai-rag` | Application RAG avec vector store et NL Query |

## Providers LLM

| Provider | Flag | Offline |
|----------|------|---------|
| OpenAI (GPT-4) | `--provider openai` | Non |
| Claude (Anthropic) | `--provider claude` | Non |
| Ollama (local) | `--provider ollama` | Oui |

## Configuration des secrets

Pour eviter de stocker des cles API dans vos fichiers :

```bash
# Via rvr env (recommande)
rvr env secrets init
rvr env secrets set "AI:OpenAI:ApiKey" "sk-..."
rvr env secrets set "AI:Anthropic:ApiKey" "sk-ant-..."

# Ou via variables d'environnement
export RVR_OPENAI_KEY="sk-..."
export RVR_ANTHROPIC_KEY="sk-ant-..."
```

## RIVORA Framework

RIVORA est un framework .NET 9 enterprise avec :

- **Clean Architecture** + DDD + CQRS (MediatR)
- **Multi-Tenancy** (colonne, schema, base separee)
- **Securite** : JWT, OAuth2/OIDC, BCrypt, 2FA/TOTP, AES-256, RGPD
- **4 providers DB** : SQL Server, PostgreSQL, MySQL, SQLite
- **IA** : RAG, NL Query Builder, AI Code Review
- **Export** : PDF, Excel, CSV
- **Observabilite** : OpenTelemetry, Prometheus, Grafana, Jaeger
- **47 projets** organises en couches modulaires

## Liens

- [Documentation](https://khalilbenaz.github.io/RIVORA/)
- [GitHub](https://github.com/khalilbenaz/RIVORA)
- [Release Notes](https://github.com/khalilbenaz/RIVORA/releases)
- [Guide : Creer son projet](https://khalilbenaz.github.io/RIVORA/guide/create-project)

## License

MIT - [Khalil Benazzouz](https://github.com/khalilbenaz)
