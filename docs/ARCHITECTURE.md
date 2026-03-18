# RIVORA Framework - Architecture Technique

> **Version** : 3.1.0 | **.NET 9** | **Clean Architecture** | **54 projets**
>
> Ce document permet a un architecte de comprendre la structure complete du framework en moins de 10 minutes.

---

## Table des matieres

1. [Vue d'ensemble](#1-vue-densemble)
2. [C4 Niveau 1 - Contexte Systeme](#2-c4-niveau-1---contexte-systeme)
3. [C4 Niveau 2 - Conteneurs](#3-c4-niveau-2---conteneurs)
4. [C4 Niveau 3 - Composants du Core](#4-c4-niveau-3---composants-du-core)
5. [Couches DDD](#5-couches-ddd)
6. [Carte des dependances inter-modules](#6-carte-des-dependances-inter-modules)
7. [Flux d'une requete HTTP](#7-flux-dune-requete-http)
8. [Architecture modulaire - Pattern IRvrModule](#8-architecture-modulaire---pattern-irvrmodule)
9. [Inventaire complet des 54 projets](#9-inventaire-complet-des-54-projets)

---

## 1. Vue d'ensemble

RIVORA est un framework entreprise .NET 9 fonde sur les principes de **Clean Architecture** et **Domain-Driven Design**. Il fournit une base modulaire couvrant l'ensemble des besoins d'une application SaaS moderne : API REST/GraphQL, multi-tenancy, securite avancee, intelligence artificielle, event sourcing, et bien plus.

**Regle fondamentale** : les dependances pointent toujours vers l'interieur. Le Domain ne depend de rien.

---

## 2. C4 Niveau 1 - Contexte Systeme

Ce diagramme montre RIVORA dans son ecosysteme, avec les acteurs externes qui interagissent avec lui.

```mermaid
graph TB
    subgraph Acteurs Externes
        CA["Applications Clientes<br/>(SPA, Mobile, Desktop)"]
        AUTH["Fournisseurs Auth Externes<br/>(Azure AD, Google, Okta)"]
        MON["Monitoring & Observabilite<br/>(Prometheus, Grafana, Seq)"]
    end

    subgraph Infrastructure Cloud
        DB["Bases de Donnees<br/>(SQL Server, PostgreSQL,<br/>MySQL, SQLite, CosmosDB, MongoDB)"]
        MQ["Message Brokers<br/>(RabbitMQ, Azure Service Bus,<br/>Kafka)"]
        STORE["Stockage<br/>(Azure Blob, AWS S3,<br/>Systeme de fichiers)"]
    end

    RVR["RIVORA Framework<br/>.NET 9 Enterprise<br/>54 projets modulaires"]

    CA -->|"REST / GraphQL / SignalR"| RVR
    AUTH -->|"OAuth 2.0 / OIDC"| RVR
    RVR -->|"Telemetrie / Logs"| MON
    RVR -->|"EF Core / Drivers natifs"| DB
    RVR -->|"Pub/Sub / Queues"| MQ
    RVR -->|"Fichiers / Blobs"| STORE

    style RVR fill:#1a5276,stroke:#2980b9,color:#fff
    style CA fill:#27ae60,stroke:#1e8449,color:#fff
    style AUTH fill:#8e44ad,stroke:#7d3c98,color:#fff
    style MON fill:#d35400,stroke:#a04000,color:#fff
    style DB fill:#2c3e50,stroke:#1a252f,color:#fff
    style MQ fill:#2c3e50,stroke:#1a252f,color:#fff
    style STORE fill:#2c3e50,stroke:#1a252f,color:#fff
```

---

## 3. C4 Niveau 2 - Conteneurs

Vue des grands conteneurs/couches qui composent le framework.

```mermaid
graph TB
    subgraph "RIVORA Framework"
        direction TB

        subgraph "Presentation"
            API["RVR.Framework.Api<br/>REST + Swagger"]
            GQL["RVR.Framework.GraphQL<br/>HotChocolate"]
            ADM["RVR.Framework.Admin<br/>Blazor Server"]
            VER["RVR.Framework.ApiVersioning"]
        end

        subgraph "Core"
            CORE["RVR.Framework.Core<br/>Abstractions de base"]
            APP["RVR.Framework.Application<br/>CQRS / MediatR / DTOs"]
            DOM["RVR.Framework.Domain<br/>Entites / Value Objects"]
            INF["RVR.Framework.Infrastructure<br/>EF Core / Repositories"]
        end

        subgraph "Data"
            DA["Data.Abstractions"]
            SQL["Data.SqlServer"]
            PG["Data.PostgreSQL"]
            MY["Data.MySQL"]
            LT["Data.SQLite"]
            CO["Data.CosmosDB"]
            MO["Data.MongoDB"]
            RR["Data.ReadReplica"]
        end

        subgraph "Securite"
            SEC["RVR.Framework.Security<br/>JWT / BCrypt / 2FA"]
            IDP["RVR.Framework.Identity.Pro"]
            PRV["RVR.Framework.Privacy<br/>RGPD"]
            AKY["RVR.Framework.ApiKeys"]
        end

        subgraph "Modules"
            MOD["Caching, Jobs, Notifications,<br/>RealTime, Storage, Search,<br/>EventSourcing, Saga, Email,<br/>Messaging, Resilience, Workflows,<br/>Plugins, Alerting, Profiling..."]
        end

        subgraph "IA"
            AI["RVR.Framework.AI<br/>RAG / LLM"]
            NQ["RVR.Framework.NaturalQuery<br/>NL vers LINQ"]
        end

        subgraph "Multi-tenancy"
            MT["RVR.Framework.MultiTenancy"]
            SAAS["RVR.Framework.SaaS"]
            BIL["RVR.Framework.Billing"]
        end

        subgraph "Hebergement"
            HOST["RVR.Framework.AppHost<br/>.NET Aspire"]
            SD["RVR.Framework.ServiceDefaults"]
        end

        subgraph "Outils"
            CLI["RVR.CLI<br/>Scaffolding + AI Review"]
            STU["RVR.Studio<br/>IDE Visuel"]
        end
    end

    API --> APP
    GQL --> APP
    APP --> DOM
    APP --> INF
    INF --> DA
    DA --> SQL
    DA --> PG
    DA --> CO
    DA --> MO

    style DOM fill:#e74c3c,stroke:#c0392b,color:#fff
    style APP fill:#f39c12,stroke:#d68910,color:#fff
    style INF fill:#3498db,stroke:#2980b9,color:#fff
    style API fill:#27ae60,stroke:#1e8449,color:#fff
```

---

## 4. C4 Niveau 3 - Composants du Core

Zoom sur le coeur du framework : les interactions entre Domain, Application et Infrastructure.

```mermaid
graph TB
    subgraph "Application Layer"
        CMD["Commands<br/>(IRequest via MediatR)"]
        QRY["Queries<br/>(IRequest via MediatR)"]
        HDL["Handlers<br/>(IRequestHandler)"]
        VAL["Validators<br/>(FluentValidation)"]
        SVC["Services Applicatifs"]
        DTO["DTOs / Mappings"]
        BEH["Pipeline Behaviors<br/>(Validation, Logging, Caching)"]
    end

    subgraph "Domain Layer"
        ENT["Entites<br/>(BaseEntity, AuditableEntity)"]
        VO["Value Objects"]
        DE["Domain Events<br/>(IDomainEvent)"]
        SPEC["Specifications<br/>(ISpecification)"]
        AGG["Aggregats"]
        DR["Regles de Domaine"]
    end

    subgraph "Infrastructure Layer"
        REPO["Repositories<br/>(IRepository, IReadRepository)"]
        DBC["DbContext<br/>(EF Core 9)"]
        EXT["Services Externes<br/>(Email, Storage, Messaging)"]
        EVTS["Event Store"]
        CACHE["Cache Distribue"]
    end

    CMD --> HDL
    QRY --> HDL
    HDL --> SVC
    HDL --> VAL
    BEH --> HDL
    SVC --> REPO
    SVC --> ENT
    SVC --> SPEC
    REPO --> DBC
    REPO --> ENT
    ENT --> DE
    ENT --> VO
    ENT --> DR
    AGG --> ENT
    DBC --> EVTS

    style ENT fill:#e74c3c,stroke:#c0392b,color:#fff
    style VO fill:#e74c3c,stroke:#c0392b,color:#fff
    style DE fill:#e74c3c,stroke:#c0392b,color:#fff
    style CMD fill:#f39c12,stroke:#d68910,color:#fff
    style QRY fill:#f39c12,stroke:#d68910,color:#fff
    style REPO fill:#3498db,stroke:#2980b9,color:#fff
    style DBC fill:#3498db,stroke:#2980b9,color:#fff
```

---

## 5. Couches DDD

Representation visuelle des 4 couches de la Clean Architecture. Les dependances pointent **toujours vers l'interieur**.

```mermaid
graph TB
    subgraph "Couche Presentation"
        direction LR
        P1["Controllers REST"]
        P2["Minimal APIs"]
        P3["GraphQL HotChocolate"]
        P4["Blazor Admin"]
        P5["SignalR RealTime"]
    end

    subgraph "Couche Application"
        direction LR
        A1["Commands / Queries<br/>(CQRS via MediatR)"]
        A2["Services Applicatifs"]
        A3["Validators<br/>(FluentValidation)"]
        A4["DTOs & Mappings"]
    end

    subgraph "Couche Domain"
        direction LR
        D1["Entites"]
        D2["Value Objects"]
        D3["Domain Events"]
        D4["Specifications"]
    end

    subgraph "Couche Infrastructure"
        direction LR
        I1["EF Core 9<br/>DbContext"]
        I2["Repositories"]
        I3["Services Externes"]
        I4["Providers de Donnees"]
    end

    P1 --> A1
    P2 --> A1
    P3 --> A1
    P4 --> A2
    P5 --> A2
    A1 --> D1
    A2 --> D1
    A3 --> D1
    I1 --> D1
    I2 --> D1
    I3 -.->|"implemente les interfaces du Domain"| D1

    style P1 fill:#27ae60,stroke:#1e8449,color:#fff
    style P2 fill:#27ae60,stroke:#1e8449,color:#fff
    style P3 fill:#27ae60,stroke:#1e8449,color:#fff
    style P4 fill:#27ae60,stroke:#1e8449,color:#fff
    style P5 fill:#27ae60,stroke:#1e8449,color:#fff
    style A1 fill:#f39c12,stroke:#d68910,color:#fff
    style A2 fill:#f39c12,stroke:#d68910,color:#fff
    style A3 fill:#f39c12,stroke:#d68910,color:#fff
    style A4 fill:#f39c12,stroke:#d68910,color:#fff
    style D1 fill:#e74c3c,stroke:#c0392b,color:#fff
    style D2 fill:#e74c3c,stroke:#c0392b,color:#fff
    style D3 fill:#e74c3c,stroke:#c0392b,color:#fff
    style D4 fill:#e74c3c,stroke:#c0392b,color:#fff
    style I1 fill:#3498db,stroke:#2980b9,color:#fff
    style I2 fill:#3498db,stroke:#2980b9,color:#fff
    style I3 fill:#3498db,stroke:#2980b9,color:#fff
    style I4 fill:#3498db,stroke:#2980b9,color:#fff
```

**Legende des couleurs :**
- **Rouge** : Domain (aucune dependance externe)
- **Orange** : Application (depend uniquement du Domain)
- **Bleu** : Infrastructure (implemente les contrats du Domain)
- **Vert** : Presentation (point d'entree, depend de Application)

---

## 6. Carte des dependances inter-modules

```mermaid
graph LR
    CORE["Core"]
    DOM["Domain"]
    APP["Application"]
    INF["Infrastructure"]

    API["Api"]
    GQL["GraphQL"]
    ADM["Admin Blazor"]
    AVER["ApiVersioning"]

    SEC["Security"]
    IDP["Identity.Pro"]
    PRV["Privacy"]
    AKY["ApiKeys"]

    DA["Data.Abstractions"]
    SQLSRV["Data.SqlServer"]
    PG["Data.PostgreSQL"]
    MY["Data.MySQL"]
    LT["Data.SQLite"]
    COSMO["Data.CosmosDB"]
    MONGO["Data.MongoDB"]
    RR["Data.ReadReplica"]

    MT["MultiTenancy"]
    SAAS["SaaS"]
    BIL["Billing"]

    CACHE["Caching"]
    JOBS["Jobs"]
    NOTIF["Notifications"]
    RT["RealTime"]
    STOR["Storage"]
    SRCH["Search"]
    ES["EventSourcing"]
    SAGA["Saga"]
    EMAIL["Email"]
    MSG["Messaging"]
    RES["Resilience"]
    WF["Workflows"]
    PLUG["Plugins"]
    ALERT["Alerting"]
    PROF["Profiling"]
    IDEM["Idempotency"]
    DAPR["Dapr"]
    HC["HealthChecks"]
    LOC["Localization"]
    FM["FeatureManagement"]
    FEAT["Features"]
    AUDIT["AuditLogging.UI"]

    AI["AI"]
    NQ["NaturalQuery"]

    EXP["Export"]
    WH["Webhooks"]
    CLT["Client"]

    HOST["AppHost"]
    SD["ServiceDefaults"]

    CLI["RVR.CLI"]
    STU["RVR.Studio"]

    %% Dependances du Core
    DOM --> CORE
    APP --> DOM
    APP --> CORE
    INF --> APP
    INF --> DA

    %% Presentation vers Application
    API --> APP
    API --> SEC
    GQL --> APP
    ADM --> APP
    AVER --> API

    %% Data providers vers abstractions
    SQLSRV --> DA
    PG --> DA
    MY --> DA
    LT --> DA
    COSMO --> DA
    MONGO --> DA
    RR --> DA

    %% Securite
    IDP --> SEC
    PRV --> SEC
    AKY --> SEC

    %% Multi-tenancy
    SAAS --> MT
    BIL --> SAAS

    %% Modules vers Core
    CACHE --> CORE
    JOBS --> CORE
    NOTIF --> CORE
    RT --> CORE
    STOR --> CORE
    ES --> DOM
    SAGA --> ES
    EMAIL --> CORE
    MSG --> CORE
    RES --> CORE
    WF --> CORE
    PLUG --> CORE
    ALERT --> CORE
    HC --> CORE
    SRCH --> CORE
    IDEM --> CACHE
    FM --> FEAT

    %% IA
    AI --> APP
    NQ --> AI

    %% Integration
    EXP --> APP
    WH --> APP
    CLT --> API

    %% Hebergement
    HOST --> SD

    style CORE fill:#1a5276,stroke:#2980b9,color:#fff
    style DOM fill:#e74c3c,stroke:#c0392b,color:#fff
    style APP fill:#f39c12,stroke:#d68910,color:#fff
    style INF fill:#3498db,stroke:#2980b9,color:#fff
    style SEC fill:#8e44ad,stroke:#7d3c98,color:#fff
```

---

## 7. Flux d'une requete HTTP

Pipeline complet du traitement d'une requete HTTP depuis le client jusqu'a la base de donnees.

```mermaid
sequenceDiagram
    participant C as Client
    participant SH as SecurityHeaders
    participant GEH as GlobalExceptionHandler
    participant LOG as SerilogRequestLogging
    participant OC as OutputCache
    participant RC as ResponseCompression
    participant ET as ETag Middleware
    participant RL as RateLimiter
    participant AK as ApiKey Auth
    participant JWT as JWT Auth
    participant AZ as Authorization
    participant MOD as RvrModules Middleware
    participant CTL as Controller / Minimal API
    participant MED as MediatR Pipeline
    participant SVC as Service Applicatif
    participant REPO as Repository
    participant DBC as DbContext (EF Core 9)
    participant DB as Base de Donnees

    C->>SH: Requete HTTP
    SH->>GEH: + En-tetes de securite
    GEH->>LOG: Try/Catch global
    LOG->>OC: Log de la requete (Serilog)
    OC->>RC: Verification du cache de sortie

    alt Cache HIT
        OC-->>C: Reponse en cache (200)
    end

    RC->>ET: Compression (gzip/brotli)
    ET->>RL: Verification ETag

    alt ETag correspond
        ET-->>C: 304 Not Modified
    end

    RL->>AK: Verification du rate limit

    alt Limite atteinte
        RL-->>C: 429 Too Many Requests
    end

    AK->>JWT: Validation cle API
    JWT->>AZ: Validation token JWT
    AZ->>MOD: Verification des permissions

    alt Non autorise
        AZ-->>C: 401/403
    end

    MOD->>CTL: Middlewares des modules RVR
    CTL->>MED: Dispatch de la Command/Query
    MED->>MED: Behavior: Validation (FluentValidation)
    MED->>MED: Behavior: Logging
    MED->>MED: Behavior: Caching
    MED->>SVC: Handler execute le service
    SVC->>REPO: Appel au repository
    REPO->>DBC: Requete LINQ
    DBC->>DB: SQL genere par EF Core
    DB-->>DBC: Resultats
    DBC-->>REPO: Entites materialisees
    REPO-->>SVC: Donnees metier
    SVC-->>MED: Resultat du handler
    MED-->>CTL: Response DTO
    CTL-->>C: HTTP 200 (JSON)
```

---

## 8. Architecture modulaire - Pattern IRvrModule

Chaque module du framework implemente l'interface `IRvrModule`, definie dans `RVR.Framework.Core.Modules`.

### Interface IRvrModule

```csharp
public interface IRvrModule
{
    string Name { get; }
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    void Configure(IApplicationBuilder app);
}
```

### Enregistrement des modules

```mermaid
graph TD
    PROG["Program.cs<br/>Point d'entree"]
    PROG -->|"builder.Services<br/>.AddRvrModules()"| REG["Decouverte des modules<br/>(Scan d'assemblies)"]
    REG --> M1["CachingModule"]
    REG --> M2["SecurityModule"]
    REG --> M3["JobsModule"]
    REG --> M4["NotificationsModule"]
    REG --> M5["RealTimeModule"]
    REG --> MN["...autres modules"]

    M1 -->|"ConfigureServices()"| DI["Conteneur DI<br/>(IServiceCollection)"]
    M2 -->|"ConfigureServices()"| DI
    M3 -->|"ConfigureServices()"| DI

    PROG -->|"app.UseRvrModules()"| PIPE["Pipeline HTTP"]
    M1 -->|"Configure()"| PIPE
    M2 -->|"Configure()"| PIPE
    M3 -->|"Configure()"| PIPE

    style PROG fill:#1a5276,stroke:#2980b9,color:#fff
    style REG fill:#f39c12,stroke:#d68910,color:#fff
    style DI fill:#27ae60,stroke:#1e8449,color:#fff
    style PIPE fill:#8e44ad,stroke:#7d3c98,color:#fff
```

**Principe** : chaque module est autonome. Il declare ses propres services et ses middlewares. L'application hote n'a qu'a appeler `AddRvrModules()` et `UseRvrModules()` pour activer l'ensemble des modules decouverts.

---

## 9. Inventaire complet des 54 projets

### Core (4 projets)

| Projet | Repertoire | Description |
|--------|-----------|-------------|
| `RVR.Framework.Core` | `src/core/` | Abstractions de base, interfaces `IRvrModule`, types communs |
| `RVR.Framework.Domain` | `src/core/` | Entites, Value Objects, Domain Events, Specifications |
| `RVR.Framework.Application` | `src/core/` | CQRS (MediatR), Services, Validators (FluentValidation), DTOs |
| `RVR.Framework.Infrastructure` | `src/core/` | EF Core 9, Repositories, implementation des contrats du Domain |

### Data Providers (8 projets)

| Projet | Repertoire | Description |
|--------|-----------|-------------|
| `RVR.Framework.Data.Abstractions` | `src/data/` | Interfaces et contrats pour les providers de donnees |
| `RVR.Framework.Data.SqlServer` | `src/data/` | Provider SQL Server |
| `RVR.Framework.Data.PostgreSQL` | `src/data/` | Provider PostgreSQL |
| `RVR.Framework.Data.MySQL` | `src/data/` | Provider MySQL |
| `RVR.Framework.Data.SQLite` | `src/data/` | Provider SQLite |
| `RVR.Framework.Data.CosmosDB` | `src/data/` | Provider Azure CosmosDB |
| `RVR.Framework.Data.MongoDB` | `src/data/` | Provider MongoDB |
| `RVR.Framework.Data.ReadReplica` | `src/data/` | Routage lecture/ecriture pour replicas |

### Presentation (4 projets)

| Projet | Repertoire | Description |
|--------|-----------|-------------|
| `RVR.Framework.Api` | `src/api/` | API REST avec Swagger/OpenAPI |
| `RVR.Framework.GraphQL` | `src/api/` | API GraphQL via HotChocolate |
| `RVR.Framework.ApiVersioning` | `src/api/` | Versioning d'API |
| `RVR.Framework.Admin` | `src/ui/` | Interface d'administration Blazor Server |

### Securite (4 projets)

| Projet | Repertoire | Description |
|--------|-----------|-------------|
| `RVR.Framework.Security` | `src/security/` | JWT, BCrypt, 2FA, Rate Limiting |
| `RVR.Framework.Identity.Pro` | `src/security/` | Gestion avancee des identites |
| `RVR.Framework.Privacy` | `src/security/` | Conformite RGPD |
| `RVR.Framework.ApiKeys` | `src/modules/` | Authentification par cles API |

### Multi-tenancy (3 projets)

| Projet | Repertoire | Description |
|--------|-----------|-------------|
| `RVR.Framework.MultiTenancy` | `src/multitenancy/` | Isolation multi-locataire |
| `RVR.Framework.SaaS` | `src/multitenancy/` | Fonctionnalites SaaS (plans, limites) |
| `RVR.Framework.Billing` | `src/multitenancy/` | Facturation et abonnements |

### Intelligence Artificielle (2 projets)

| Projet | Repertoire | Description |
|--------|-----------|-------------|
| `RVR.Framework.AI` | `src/ai/` | RAG, integration LLM |
| `RVR.Framework.NaturalQuery` | `src/ai/` | Requetes en langage naturel vers LINQ |

### Modules (22 projets)

| Projet | Repertoire | Description |
|--------|-----------|-------------|
| `RVR.Framework.Caching` | `src/modules/` | Cache en memoire et distribue |
| `RVR.Framework.Jobs` | `src/modules/` | Jobs planifies (Abstractions + Hangfire + Quartz) |
| `RVR.Framework.Notifications` | `src/modules/` | Notifications multi-canal |
| `RVR.Framework.RealTime` | `src/modules/` | Communication temps reel (SignalR) |
| `RVR.Framework.Storage` | `src/modules/` | Stockage de fichiers (local, cloud) |
| `RVR.Framework.Features` | `src/modules/` | Feature flags |
| `RVR.Framework.FeatureManagement` | `src/modules/` | Gestion avancee des fonctionnalites |
| `RVR.Framework.HealthChecks` | `src/modules/` | Verifications de sante |
| `RVR.Framework.Localization.Dynamic` | `src/modules/` | Localisation dynamique |
| `RVR.Framework.EventSourcing` | `src/modules/` | Event Sourcing |
| `RVR.Framework.Saga` | `src/modules/` | Orchestration de sagas |
| `RVR.Framework.Email` | `src/modules/` | Envoi d'emails (SMTP, SendGrid) |
| `RVR.Framework.Messaging` | `src/modules/` | Bus de messages |
| `RVR.Framework.Resilience` | `src/modules/` | Polly, Circuit Breaker, Retry |
| `RVR.Framework.Idempotency` | `src/modules/` | Garantie d'idempotence |
| `RVR.Framework.Dapr` | `src/modules/` | Integration Dapr |
| `RVR.Framework.Profiling` | `src/modules/` | Profilage de performance |
| `RVR.Framework.Workflows` | `src/modules/` | Moteur de workflows |
| `RVR.Framework.Plugins` | `src/modules/` | Systeme de plugins |
| `RVR.Framework.Search` | `src/modules/` | Recherche full-text |
| `RVR.Framework.Alerting` | `src/modules/` | Alertes et notifications systeme |
| `RVR.Framework.AuditLogging.UI` | `src/ui/` | Interface d'audit logging |

### Integration (3 projets)

| Projet | Repertoire | Description |
|--------|-----------|-------------|
| `RVR.Framework.Export` | `src/integration/` | Export PDF, Excel, CSV |
| `RVR.Framework.Webhooks` | `src/integration/` | Webhooks sortants et entrants |
| `RVR.Framework.Client` | `src/integration/` | Client HTTP type pour consommer les APIs RIVORA |

### Hebergement (2 projets)

| Projet | Repertoire | Description |
|--------|-----------|-------------|
| `RVR.Framework.AppHost` | `src/hosting/` | Orchestration .NET Aspire |
| `RVR.Framework.ServiceDefaults` | `src/hosting/` | Configuration par defaut des services |

### Outils (2 projets)

| Projet | Repertoire | Description |
|--------|-----------|-------------|
| `RVR.CLI` | `tools/` | CLI pour scaffolding et revue de code par IA |
| `RVR.Studio` | `tools/` | IDE visuel pour le framework |

---

> **Total : 54 projets** (4 Core + 8 Data + 4 Presentation + 4 Securite + 3 Multi-tenancy + 2 IA + 22 Modules + 3 Integration + 2 Hebergement + 2 Outils)
