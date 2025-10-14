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
```

### DevOps

```bash
rvr doctor       # Verifie SDK, DB, Redis, etc.
rvr benchmark    # Benchmark des performances
rvr migrate      # Appliquer les migrations
```

### Templates disponibles

| Template | Description |
|----------|-------------|
| `saas-starter` | Application SaaS multi-tenant complete |
| `api-minimal` | API REST minimale |
| `microservices` | Architecture microservices avec gateway |
| `ai-rag` | Application RAG avec vector store |
