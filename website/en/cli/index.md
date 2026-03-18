# RVR CLI

## Installation

```bash
dotnet tool install --global RVR.CLI
```

## Commands

### Scaffolding

```bash
rvr new MySaaS --template saas-starter
rvr generate crud Invoice --props "Reference:string,Amount:decimal"
rvr add-module Inventory
rvr remove-module Caching              # Cleanly remove a module
rvr remove-module Caching --dry-run    # Preview without modifying
```

### AI Review

```bash
rvr ai review --all              # All analyzers
rvr ai review --architecture     # Clean Architecture conformance
rvr ai review --ddd              # DDD anti-patterns
rvr ai review --performance      # N+1, missing async, EF anti-patterns
rvr ai review --security         # OWASP vulnerabilities
rvr ai review --provider ollama  # Offline mode
rvr ai review --output sarif     # CI/CD integration
```

### AI Chat & Generation

```bash
rvr ai chat --provider claude
rvr ai generate "Create a payment service with Stripe integration"
rvr ai design --provider openai
```

### Database

```bash
# Migrations
rvr migrate generate AddOrderTable
rvr migrate apply
rvr migrate list
rvr migrate rollback

# Seeding
rvr seed --profile demo               # Seed with demo profile
rvr seed --reset --profile test        # Truncate + reseed
rvr seed --dry-run                     # Preview without executing
rvr generate seed Product              # Scaffold a data seeder
```

### Environments & Secrets

```bash
rvr env list                          # List available environments
rvr env set DB:Host "localhost"       # Set a config value
rvr env diff Development Production   # Compare two environments
rvr env secrets init                  # Initialize User Secrets
rvr env export --format dotenv        # Export to .env
rvr env import --file .env            # Import from .env
```

### Publishing

```bash
rvr publish --target docker           # Docker image (build + push)
rvr publish --target nuget            # NuGet packages (pack + push)
rvr publish --target self-contained   # Standalone binaries
rvr publish --dry-run                 # Preview commands
```

### Upgrade

```bash
rvr upgrade --list                    # List available migrations
rvr upgrade --to 4.0 --dry-run       # Preview migration
rvr upgrade --to 4.0                  # Apply migration
```

### DevOps

```bash
rvr doctor       # Check SDK, DB, Redis, etc.
rvr benchmark    # Performance benchmarks
rvr dev          # Development server
```

### Available Templates

| Template | Description |
|----------|-------------|
| `saas-starter` | Complete multi-tenant SaaS application |
| `api-minimal` | Minimal REST API |
| `microservices` | Microservices architecture with gateway |
| `ai-rag` | RAG application with vector store |
