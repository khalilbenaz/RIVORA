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
```

### DevOps

```bash
rvr doctor       # Check SDK, DB, Redis, etc.
rvr benchmark    # Performance benchmarks
rvr migrate      # Apply migrations
```

### Available Templates

| Template | Description |
|----------|-------------|
| `saas-starter` | Complete multi-tenant SaaS application |
| `api-minimal` | Minimal REST API |
| `microservices` | Microservices architecture with gateway |
| `ai-rag` | RAG application with vector store |
