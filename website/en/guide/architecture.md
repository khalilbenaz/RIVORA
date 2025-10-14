# Architecture

## Clean Architecture .NET 9

RIVORA Framework follows a strict layered architecture with 47 projects:

```
Presentation (API)           Controllers, Minimal APIs, gRPC, SignalR, GraphQL
       |
Application (CQRS)          Commands, Queries, Validators, DTOs (MediatR)
       |
Domain (Business)            Entities, Value Objects, Domain Events, Specifications
       |
Infrastructure (Data)       EF Core 9, Repositories, External Services
```

**Fundamental rule**: dependencies always point inward. Domain depends on nothing. Infrastructure depends on Domain and Application, never the reverse.

## Project Structure

| Layer | Projects | Role |
|-------|----------|------|
| **Core** | `Core`, `Domain` | Abstractions, entities, events, specifications |
| **Application** | `Application` | CQRS (MediatR), services, validators, DTOs |
| **Infrastructure** | `Infrastructure`, `Data.*` (4 providers) | EF Core, repositories, external services |
| **Presentation** | `Api`, `GraphQL`, `Admin`, `RealTime` | REST, GraphQL, Blazor, SignalR |
| **Security** | `Security`, `Identity.Pro` | JWT, BCrypt, 2FA, Rate Limiting, Audit |
| **Multi-Tenancy** | `MultiTenancy`, `SaaS` | Tenant isolation, billing |
| **Cross-Cutting** | `Caching`, `Jobs.*`, `HealthChecks`, `Notifications`, `Storage` | Cross-cutting modules |
| **AI** | `AI`, `NaturalQuery` | RAG, vector store, NL Query Builder |
| **Integration** | `Export`, `Webhooks`, `Client` | PDF/Excel/CSV, SaaS webhooks, typed API client |
| **DevOps** | `AppHost`, `ServiceDefaults` | .NET Aspire orchestration |
| **Tools** | `RVR.CLI`, `RVR.Studio` | CLI scaffolding + AI, visual IDE |

## Modularity

Each module implements `IRvrModule` for standardized registration:

```csharp
builder.Services.AddRvrModule<CachingModule>(builder.Configuration);
builder.Services.AddRvrModule<SecurityModule>(builder.Configuration);
builder.Services.AddRvrModule<MultiTenancyModule>(builder.Configuration);
```

This approach allows scaling from a modular monolith to microservices without major refactoring.
