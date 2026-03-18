# RVR CLI

## Installation

```bash
dotnet tool install --global RVR.CLI
```

## Commandes

### Scaffolding

```bash
# Creer un nouveau projet
rvr new MySaaS --template saas-starter

# Generer un CRUD complet (Entity + Command + Query + Controller + Tests)
rvr generate crud Invoice --props "Reference:string,Amount:decimal,DueDate:DateTime"

# Ajouter un module
rvr add-module Inventory

# Retirer un module proprement
rvr remove-module Caching
rvr remove-module Caching --dry-run    # Previsualiser sans modifier
```

### AI Review

```bash
# Tous les analyzers
rvr ai review --all

# Analyzers individuels
rvr ai review --architecture    # Clean Architecture conformance
rvr ai review --ddd             # DDD anti-patterns
rvr ai review --performance     # N+1, missing async, EF anti-patterns
rvr ai review --security        # Vulnerabilites OWASP

# Options
rvr ai review --provider ollama                         # Mode offline
rvr ai review --output sarif --output-file report.sarif # CI/CD integration
```

### AI Chat & Generation

```bash
rvr ai chat --provider claude
rvr ai generate "Create a payment service with Stripe integration"
rvr ai design --provider openai
```

### Base de donnees

```bash
# Migrations
rvr migrate generate AddOrderTable
rvr migrate apply
rvr migrate list
rvr migrate rollback

# Seeding
rvr seed --profile demo               # Seeder avec le profil demo
rvr seed --reset --profile test        # Truncate + reseed
rvr seed --dry-run                     # Previsualiser
rvr generate seed Product              # Scaffolder un seeder
```

### Environnements & Secrets

```bash
rvr env list                          # Lister les environnements
rvr env set DB:Host "localhost"       # Definir une variable
rvr env diff Development Production   # Comparer deux envs
rvr env secrets init                  # Initialiser User Secrets
rvr env export --format dotenv        # Exporter en .env
rvr env import --file .env            # Importer depuis .env
```

### Publication

```bash
rvr publish --target docker           # Image Docker
rvr publish --target nuget            # Packages NuGet
rvr publish --target self-contained   # Binaires autonomes
rvr publish --dry-run                 # Previsualiser
```

### Upgrade

```bash
rvr upgrade --list                    # Migrations disponibles
rvr upgrade --to 4.0 --dry-run       # Previsualiser
rvr upgrade --to 4.0                  # Migrer
```

### DevOps

```bash
rvr doctor       # Verifie SDK, DB, Redis, etc.
rvr benchmark    # Benchmark des performances
rvr dev          # Serveur de developpement
```

### Templates disponibles

| Template | Description |
|----------|-------------|
| `saas-starter` | Application SaaS multi-tenant complete |
| `api-minimal` | API REST minimale |
| `microservices` | Architecture microservices avec gateway |
| `ai-rag` | Application RAG avec vector store |
