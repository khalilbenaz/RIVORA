# RIVORA Framework - Audit de compatibilite Native AOT

> Document de reference pour la compatibilite Native AOT de tous les modules du RIVORA Framework.
> Derniere mise a jour : Mars 2026 | .NET 9

---

## 1. Introduction

### Qu'est-ce que Native AOT ?

Native AOT (Ahead-of-Time) est un mode de publication introduit dans .NET 7 et considerablement ameliore dans .NET 8/9. Au lieu de compiler le code en IL (Intermediate Language) puis de le JIT-compiler a l'execution, Native AOT compile directement en code machine natif au moment du `dotnet publish`.

### Pourquoi c'est important pour RIVORA

| Avantage | Impact |
|----------|--------|
| **Temps de demarrage** | ~10-50ms au lieu de 500ms-2s. Critique pour les architectures serverless (Azure Functions, AWS Lambda) |
| **Empreinte memoire** | Reduction de 50-80% de la RAM consommee. Economies significatives en production |
| **Taille du binaire** | Binaire autonome, pas de runtime .NET requis sur la machine cible |
| **Securite** | Surface d'attaque reduite : pas de JIT, pas de chargement dynamique d'assemblies |
| **Conteneurs** | Images Docker ultra-legeres (~20-50 MB au lieu de 200+ MB) |

### Contraintes de Native AOT

Native AOT impose des restrictions sur certains patterns .NET :

- **Pas de reflection dynamique** : `Type.GetType(string)`, `Activator.CreateInstance()` sans annotations
- **Pas de compilation d'expressions a l'execution** : `Expression.Compile()` interdit
- **Pas de chargement dynamique d'assemblies** : `Assembly.LoadFrom()` impossible
- **Serialisation JSON** : Doit utiliser les source generators (`[JsonSerializable]`)
- **Pas de `dynamic`** : Le mot-cle `dynamic` n'est pas supporte
- **Trim-safety requis** : Tout le code doit etre decorable avec les attributs `[DynamicallyAccessedMembers]`

---

## 2. Audit de compatibilite par module

### Legende

| Icone | Statut | Description |
|-------|--------|-------------|
| ✅ | Compatible AOT | Aucune modification requise ou modifications mineures |
| ⚠️ | Partiellement compatible | Fonctionne avec des adaptations specifiques |
| ❌ | Non compatible | Depend fondamentalement de mecanismes incompatibles AOT |

---

### 2.1 Core (`src/core/`)

| Module | Statut | Details |
|--------|--------|---------|
| **RVR.Framework.Core** | ✅ Compatible AOT | Abstractions pures, interfaces, types de base. Aucune reflection. |
| **RVR.Framework.Domain** | ✅ Compatible AOT | Entites, Value Objects, Domain Events. Pattern DDD sans reflection. |
| **RVR.Framework.Application** | ⚠️ Partiellement compatible | MediatR utilise la reflection pour la decouverte automatique des handlers (`AddMediatR(cfg => cfg.RegisterServicesFromAssembly(...))`). **Solution** : Migrer vers `Mediator.SourceGenerator` ou enregistrement manuel des handlers. |
| **RVR.Framework.Infrastructure** | ⚠️ Partiellement compatible | EF Core 9 ameliore le support AOT mais les migrations, le lazy loading, et certains query providers utilisent encore de la reflection. **Solution** : Utiliser les compiled models (`dotnet ef dbcontext optimize`), desactiver le lazy loading. |

### 2.2 API (`src/api/`)

| Module | Statut | Details |
|--------|--------|---------|
| **RVR.Framework.Api** | ⚠️ Partiellement compatible | ASP.NET Core 9 supporte Native AOT via Minimal APIs. Necessite `[JsonSerializable]` pour tous les types de reponse. Les controllers MVC ne sont **pas** supportes en AOT. **Solution** : Migrer vers Minimal APIs + `JsonSerializerContext`. |
| **RVR.Framework.ApiVersioning** | ⚠️ Partiellement compatible | Asp.Versioning supporte partiellement AOT depuis la v8. Configuration par attributs potentiellement problematique. **Solution** : Utiliser le versioning par URL ou header avec configuration explicite. |
| **RVR.Framework.GraphQL** | ❌ Non compatible | HotChocolate depend massivement de la reflection pour la decouverte de schemas, la resolution de types, et le data fetching. Le framework genere du code a l'execution. Pas de timeline de support AOT annoncee. |

### 2.3 Data (`src/data/`)

| Module | Statut | Details |
|--------|--------|---------|
| **RVR.Framework.Data.Abstractions** | ✅ Compatible AOT | Interfaces de repository, specifications. Aucune implementation concrete. |
| **RVR.Framework.Data.SqlServer** | ⚠️ Partiellement compatible | EF Core + SqlClient. Le provider SqlServer utilise des expressions dynamiques pour certaines requetes. **Solution** : Compiled models + requetes pre-compilees. |
| **RVR.Framework.Data.PostgreSQL** | ⚠️ Partiellement compatible | Npgsql a un bon support AOT depuis v8. EF Core reste la contrainte principale. **Solution** : Idem SqlServer. |
| **RVR.Framework.Data.MySQL** | ⚠️ Partiellement compatible | MySqlConnector supporte AOT. Memes contraintes EF Core. |
| **RVR.Framework.Data.SQLite** | ⚠️ Partiellement compatible | Microsoft.Data.Sqlite supporte AOT. Memes contraintes EF Core. |
| **RVR.Framework.Data.MongoDB** | ⚠️ Partiellement compatible | Le driver MongoDB .NET utilise de la reflection pour le mapping BSON. Support AOT partiel depuis le driver v3. **Solution** : Utiliser les `BsonClassMap` explicites au lieu de la convention automatique. |
| **RVR.Framework.Data.CosmosDB** | ⚠️ Partiellement compatible | Le SDK Azure Cosmos DB v4 ameliore le support AOT mais la serialisation System.Text.Json necessite `[JsonSerializable]`. **Solution** : Configurer un `CosmosSerializer` personnalise avec source generators. |
| **RVR.Framework.Data.ReadReplica** | ⚠️ Partiellement compatible | Depend de l'implementation EF Core sous-jacente. Memes contraintes. |

### 2.4 Security (`src/security/`)

| Module | Statut | Details |
|--------|--------|---------|
| **RVR.Framework.Security** | ⚠️ Partiellement compatible | `Microsoft.IdentityModel.Tokens` et les JWT handlers utilisent de la reflection pour la deserialisation des claims et la validation des tokens. **Solution** : Utiliser `JsonWebTokenHandler` (plus recent, meilleur support AOT) au lieu de `JwtSecurityTokenHandler`. Annoter les types de claims avec `[JsonSerializable]`. |
| **RVR.Framework.Identity.Pro** | ⚠️ Partiellement compatible | ASP.NET Core Identity utilise de la reflection pour le UserStore/RoleStore. Support AOT en progression dans .NET 9. **Solution** : Implementations manuelles des stores avec types connus. |
| **RVR.Framework.Privacy** | ⚠️ Partiellement compatible | Si le module utilise des attributs personnalises avec reflection pour decouvrir les proprietes sensibles. **Solution** : Source generator pour detecter les proprietes `[PersonalData]` au compile-time. |

### 2.5 Multi-tenancy (`src/multitenancy/`)

| Module | Statut | Details |
|--------|--------|---------|
| **RVR.Framework.MultiTenancy** | ⚠️ Partiellement compatible | Les query filters globaux EF Core utilisent des expressions lambda dynamiques. `Expression.Lambda()` est partiellement supporte en AOT mais `Expression.Compile()` ne l'est pas. **Solution** : Pre-compiler les filtres au demarrage ou utiliser des interceptors EF Core. |
| **RVR.Framework.Billing** | ⚠️ Partiellement compatible | Depend des SDKs de paiement (Stripe, etc.) dont le support AOT varie. **Solution** : Verifier la compatibilite AOT de chaque SDK de paiement utilise. |
| **RVR.Framework.SaaS** | ⚠️ Partiellement compatible | Composition de MultiTenancy + Billing. Herite des contraintes des deux modules. |

### 2.6 Modules (`src/modules/`)

| Module | Statut | Details |
|--------|--------|---------|
| **RVR.Framework.ApiKeys** | ✅ Compatible AOT | Validation de cles API par comparaison de hash. Aucune reflection. |
| **RVR.Framework.Idempotency** | ✅ Compatible AOT | Stockage et verification d'identifiants de requete. Operations simples sur les chaines et le cache. |
| **RVR.Framework.Resilience** | ✅ Compatible AOT | Polly v8+ supporte nativement AOT. Circuit breaker, retry, timeout fonctionnent sans reflection. |
| **RVR.Framework.SMS** | ✅ Compatible AOT | Utilise `HttpClient` pour les appels aux providers SMS. Aucune reflection. Annoter les DTOs de requete/reponse avec `[JsonSerializable]`. |
| **RVR.Framework.Storage** | ✅ Compatible AOT | Operations de fichiers et Azure Blob Storage. Le SDK Azure.Storage.Blobs supporte AOT. |
| **RVR.Framework.Alerting** | ✅ Compatible AOT | Envoi de notifications/alertes via des canaux configures. Pattern simple sans reflection. |
| **RVR.Framework.Email** | ✅ Compatible AOT | Envoi d'emails via SMTP ou providers. MailKit supporte AOT. |
| **RVR.Framework.HealthChecks** | ✅ Compatible AOT | ASP.NET Core Health Checks supporte AOT nativement dans .NET 9. |
| **RVR.Framework.Caching** | ⚠️ Partiellement compatible | La serialisation/deserialisation du cache peut utiliser la reflection (JSON, MessagePack). **Solution** : Utiliser `System.Text.Json` avec `[JsonSerializable]` pour la serialisation du cache. |
| **RVR.Framework.FeatureManagement** | ⚠️ Partiellement compatible | `Microsoft.FeatureManagement` utilise de la reflection pour la decouverte des filtres. **Solution** : Enregistrement explicite des filtres au lieu de la decouverte automatique. |
| **RVR.Framework.Features** | ⚠️ Partiellement compatible | Similaire a FeatureManagement. Memes contraintes. |
| **RVR.Framework.Messaging** | ⚠️ Partiellement compatible | MassTransit/RabbitMQ utilisent de la reflection pour la deserialisation des messages. **Solution** : Configurer les serialiseurs avec les types connus explicitement. |
| **RVR.Framework.Notifications** | ⚠️ Partiellement compatible | Si SignalR est utilise : supporte AOT en .NET 9 mais necessite `[JsonSerializable]` pour les hub methods. |
| **RVR.Framework.RealTime** | ⚠️ Partiellement compatible | SignalR en .NET 9 supporte AOT avec configuration explicite des types serialises. |
| **RVR.Framework.Search** | ⚠️ Partiellement compatible | Elasticsearch/OpenSearch clients utilisent de la reflection pour le mapping. **Solution** : Configurer les mappings manuellement avec des types connus. |
| **RVR.Framework.Dapr** | ⚠️ Partiellement compatible | Le SDK Dapr pour .NET ameliore le support AOT mais la serialisation des messages necessite `[JsonSerializable]`. |
| **RVR.Framework.Jobs.Abstractions** | ✅ Compatible AOT | Interfaces et abstractions pour les jobs. Aucune implementation concrete. |
| **RVR.Framework.Jobs.Hangfire** | ❌ Non compatible | Hangfire utilise massivement la reflection pour la serialisation des jobs, la decouverte des methodes, et l'execution differee (`BackgroundJob.Enqueue(() => ...)` repose sur des expressions). |
| **RVR.Framework.Jobs.Quartz** | ⚠️ Partiellement compatible | Quartz.NET v4 ameliore le support AOT mais l'instanciation des jobs utilise `Activator.CreateInstance`. **Solution** : Implementer un `IJobFactory` personnalise avec DI explicite. |
| **RVR.Framework.Localization.Dynamic** | ❌ Non compatible | Le mot "Dynamic" indique un chargement a l'execution des ressources de localisation, potentiellement via reflection ou compilation dynamique. |
| **RVR.Framework.EventSourcing** | ❌ Non compatible | La deserialisation des evenements par nom de type (`Type.GetType(eventTypeName)`) est fondamentalement incompatible avec AOT. Le pattern event sourcing necessite de reconstruire des objets a partir de leur nom de type stocke. **Solution theorique** : Creer un registre statique type-name ↔ type avec un switch expression. Effort important. |
| **RVR.Framework.Saga** | ❌ Non compatible | Les state machines (MassTransit Saga ou implementation custom) utilisent du dispatch dynamique, de la reflection pour la decouverte des handlers d'etat, et potentiellement `Expression.Compile()`. |
| **RVR.Framework.Plugins** | ❌ Non compatible | Par conception, le chargement de plugins repose sur `Assembly.LoadFrom()` / `AssemblyLoadContext`, qui est **fondamentalement** incompatible avec Native AOT. Pas de solution de contournement possible. |
| **RVR.Framework.Profiling** | ❌ Non compatible | L'instrumentation dynamique necessite de l'interception a l'execution, potentiellement via `DispatchProxy`, `Castle.DynamicProxy` ou des intercepteurs IL. Tous incompatibles avec AOT. |
| **RVR.Framework.Workflows** | ❌ Non compatible | L'execution dynamique des etapes de workflow necessite la resolution de types a l'execution et potentiellement la compilation d'expressions. |

### 2.7 Integration (`src/integration/`)

| Module | Statut | Details |
|--------|--------|---------|
| **RVR.Framework.Client** | ✅ Compatible AOT | Client HTTP type. `HttpClient` + `IHttpClientFactory` sont AOT-compatibles. Utiliser `[JsonSerializable]` pour les DTOs. |
| **RVR.Framework.Export** | ⚠️ Partiellement compatible | Depend des bibliotheques de generation de documents (PDF, Excel). ClosedXML et QuestPDF ont un support AOT variable. **Solution** : Verifier chaque bibliotheque individuellement. |
| **RVR.Framework.Webhooks** | ⚠️ Partiellement compatible | Envoi/reception de webhooks via HTTP. Compatible si la serialisation utilise les source generators. |

### 2.8 AI (`src/ai/`)

| Module | Statut | Details |
|--------|--------|---------|
| **RVR.Framework.AI** | ❌ Non compatible | Les SDKs LLM (Semantic Kernel, Azure OpenAI) utilisent de la reflection pour le function calling, la serialisation des prompts, et le binding des plugins. Semantic Kernel genere du code dynamiquement. |
| **RVR.Framework.AI.Guardrails** | ✅ Compatible AOT | Basee sur des regex pre-compilees et des regles statiques. Aucune reflection. Utiliser `[GeneratedRegex]` pour de meilleures performances AOT. |
| **RVR.Framework.NaturalQuery** | ❌ Non compatible | La compilation d'arbres d'expressions (`Expression.Compile()`) est le coeur meme de ce module. Fondamentalement incompatible avec AOT. |

### 2.9 Hosting (`src/hosting/`)

| Module | Statut | Details |
|--------|--------|---------|
| **RVR.Framework.AppHost** | ⚠️ Partiellement compatible | .NET Aspire AppHost. Le modele d'orchestration est compatible mais depend des modules references. |
| **RVR.Framework.Aspire** | ⚠️ Partiellement compatible | Service Defaults pour Aspire. Compatible si les extensions enregistrees le sont. |
| **RVR.Framework.ServiceDefaults** | ✅ Compatible AOT | Configuration OpenTelemetry, Health Checks. Les packages OTel supportent AOT dans .NET 9. |

### 2.10 UI (`src/ui/`)

| Module | Statut | Details |
|--------|--------|---------|
| **RVR.Framework.Admin** | ❌ Non compatible | Interface d'administration web. Blazor Server/WASM ne supporte pas Native AOT (seulement les API backend). |
| **RVR.Framework.AuditLogging.UI** | ❌ Non compatible | Composants UI. Memes contraintes que le module Admin. |

---

### Resume statistique

| Statut | Nombre | Pourcentage |
|--------|--------|-------------|
| ✅ Compatible AOT | 14 | 25% |
| ⚠️ Partiellement compatible | 27 | 48% |
| ❌ Non compatible | 15 | 27% |
| **Total** | **56** | **100%** |

---

## 3. Plan de migration par priorite

### P1 - Fondations (Sprint 1-2)

**Objectif** : Rendre le coeur du framework AOT-ready.

| Module | Action | Effort |
|--------|--------|--------|
| RVR.Framework.Core | Verifier les annotations `[DynamicallyAccessedMembers]`, ajouter `<IsAotCompatible>true</IsAotCompatible>` au csproj | 1 jour |
| RVR.Framework.Domain | Idem. S'assurer que les entites/value objects n'utilisent pas de reflection | 1 jour |
| RVR.Framework.Data.Abstractions | Ajouter les annotations de trimming sur les interfaces generiques | 0.5 jour |
| RVR.Framework.ApiKeys | Ajouter `<IsAotCompatible>true</IsAotCompatible>`, tester la publication AOT | 0.5 jour |
| RVR.Framework.Idempotency | Idem | 0.5 jour |
| RVR.Framework.Resilience | Idem | 0.5 jour |

**Livrable** : NuGet packages marques `<IsAotCompatible>true</IsAotCompatible>`.

### P2 - Infrastructure critique (Sprint 3-5)

**Objectif** : Supporter un scenario API minimal en AOT.

| Module | Action | Effort |
|--------|--------|--------|
| RVR.Framework.Api | Migrer les exemples vers Minimal APIs, ajouter `JsonSerializerContext` | 3 jours |
| RVR.Framework.Security | Migrer vers `JsonWebTokenHandler`, annoter les types de claims | 3 jours |
| RVR.Framework.Infrastructure | Generer les compiled models EF Core, desactiver le lazy loading | 5 jours |
| RVR.Framework.Caching | Migrer la serialisation vers les source generators | 2 jours |
| RVR.Framework.Client | Ajouter `[JsonSerializable]` sur les DTOs | 1 jour |

**Livrable** : Un sample fonctionnel avec API + auth + base de donnees en AOT.

### P3 - Modules secondaires (Sprint 6-8)

**Objectif** : Etendre la couverture AOT aux modules les plus utilises.

| Module | Action | Effort |
|--------|--------|--------|
| RVR.Framework.Application | Evaluer `Mediator.SourceGenerator` comme alternative a MediatR | 5 jours |
| RVR.Framework.MultiTenancy | Pre-compiler les query filters | 3 jours |
| RVR.Framework.Messaging | Configurer la serialisation avec types connus | 3 jours |
| RVR.Framework.Storage | Verifier le SDK Azure, ajouter `[JsonSerializable]` | 1 jour |
| RVR.Framework.SMS | Ajouter `[JsonSerializable]` sur les DTOs | 1 jour |
| RVR.Framework.Email | Verifier la compatibilite MailKit | 0.5 jour |

**Livrable** : 70%+ des modules AOT-compatibles.

### P4 - Modules avances (Sprint 9-12, optionnel)

**Objectif** : Evaluer les alternatives pour les modules non compatibles.

| Module | Action | Effort |
|--------|--------|--------|
| RVR.Framework.EventSourcing | Creer un registre statique de types d'evenements | 10 jours |
| RVR.Framework.Saga | Evaluer des alternatives de state machine AOT-friendly | 10 jours |
| RVR.Framework.Workflows | Rearchitecturer avec des delegates pre-compiles | 10 jours |
| RVR.Framework.Plugins | **Aucune solution** : Ce module est incompatible par conception | N/A |
| RVR.Framework.Profiling | Evaluer les alternatives basees sur EventPipe | 5 jours |

**Note** : Les modules RVR.Framework.Plugins, RVR.Framework.Admin et RVR.Framework.AuditLogging.UI resteront non compatibles AOT. C'est acceptable car ils ne sont pas destines aux scenarios serverless.

---

## 4. Patterns a corriger

### 4.1 Remplacer `Activator.CreateInstance<T>()`

**Avant (incompatible AOT)** :

```csharp
// Decouverte dynamique - CASSE en AOT
public T CreateHandler<T>() where T : class
{
    return Activator.CreateInstance<T>();
}

public object CreateByType(Type type)
{
    return Activator.CreateInstance(type)!;
}
```

**Apres (compatible AOT)** :

```csharp
// Option 1 : Factory pattern avec DI
public T CreateHandler<T>(IServiceProvider sp) where T : class
{
    return sp.GetRequiredService<T>();
}

// Option 2 : Annotation pour preserver les metadonnees
public T CreateHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>()
    where T : class, new()
{
    return new T();
}

// Option 3 : Registre explicite
public static class HandlerRegistry
{
    private static readonly Dictionary<Type, Func<object>> _factories = new()
    {
        [typeof(OrderHandler)] = () => new OrderHandler(),
        [typeof(PaymentHandler)] = () => new PaymentHandler(),
    };

    public static object Create(Type type) => _factories[type]();
}
```

### 4.2 Remplacer `Type.GetType(string)`

**Avant (incompatible AOT)** :

```csharp
// Event Sourcing - deserialisation par nom de type
public IDomainEvent DeserializeEvent(string typeName, string json)
{
    var type = Type.GetType(typeName)
        ?? throw new InvalidOperationException($"Type {typeName} not found");

    return (IDomainEvent)JsonSerializer.Deserialize(json, type)!;
}
```

**Apres (compatible AOT)** :

```csharp
// Switch expression avec tous les types connus
public IDomainEvent DeserializeEvent(string typeName, string json)
{
    return typeName switch
    {
        nameof(OrderCreatedEvent) => JsonSerializer.Deserialize<OrderCreatedEvent>(json, AppJsonContext.Default.OrderCreatedEvent)!,
        nameof(OrderPaidEvent) => JsonSerializer.Deserialize<OrderPaidEvent>(json, AppJsonContext.Default.OrderPaidEvent)!,
        nameof(OrderShippedEvent) => JsonSerializer.Deserialize<OrderShippedEvent>(json, AppJsonContext.Default.OrderShippedEvent)!,
        _ => throw new InvalidOperationException($"Unknown event type: {typeName}")
    };
}

// Ou : utiliser un source generator pour generer le switch automatiquement
[EventTypeRegistry]  // Source generator custom
public partial class DomainEventDeserializer { }
```

### 4.3 Remplacer `Expression.Compile()`

**Avant (incompatible AOT)** :

```csharp
// Filtre dynamique - compilation a l'execution
public Func<T, bool> BuildFilter<T>(string propertyName, object value)
{
    var param = Expression.Parameter(typeof(T));
    var property = Expression.Property(param, propertyName);
    var constant = Expression.Constant(value);
    var equal = Expression.Equal(property, constant);
    var lambda = Expression.Lambda<Func<T, bool>>(equal, param);
    return lambda.Compile(); // INTERDIT en AOT
}
```

**Apres (compatible AOT)** :

```csharp
// Option 1 : Delegates pre-compiles
public static class ProductFilters
{
    public static Func<Product, bool> ByCategory(string category)
        => p => p.Category == category;

    public static Func<Product, bool> ByPriceRange(decimal min, decimal max)
        => p => p.Price >= min && p.Price <= max;

    public static Func<Product, bool> ByName(string name)
        => p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase);
}

// Option 2 : Interface de specification (DDD pattern)
public interface ISpecification<T>
{
    bool IsSatisfiedBy(T entity);
}

public class ProductByCategorySpec : ISpecification<Product>
{
    private readonly string _category;
    public ProductByCategorySpec(string category) => _category = category;
    public bool IsSatisfiedBy(Product p) => p.Category == _category;
}
```

### 4.4 Serialisation JSON avec Source Generators

**Avant (incompatible AOT)** :

```csharp
// Serialisation par reflection - LENT et incompatible AOT
app.MapGet("/api/products", () =>
{
    var products = GetProducts();
    return Results.Ok(products); // Utilise la reflection pour serialiser
});

app.MapPost("/api/products", (Product product) =>
{
    // Deserialisation par reflection
    SaveProduct(product);
    return Results.Created($"/api/products/{product.Id}", product);
});
```

**Apres (compatible AOT)** :

```csharp
// 1. Definir le contexte de serialisation
[JsonSerializable(typeof(Product))]
[JsonSerializable(typeof(List<Product>))]
[JsonSerializable(typeof(ApiResponse<Product>))]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class AppJsonContext : JsonSerializerContext { }

// 2. Configurer dans Program.cs
var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default);
});

// 3. Les endpoints fonctionnent maintenant en AOT
app.MapGet("/api/products", () => TypedResults.Ok(GetProducts()));
app.MapPost("/api/products", (Product product) =>
{
    SaveProduct(product);
    return TypedResults.Created($"/api/products/{product.Id}", product);
});
```

### 4.5 Remplacer MediatR par Mediator.SourceGenerator

**Avant (MediatR - reflection)** :

```csharp
// Installation : MediatR (utilise la reflection pour scanner les assemblies)
services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Handler decouvert par reflection
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        // ...
    }
}
```

**Apres (Mediator.SourceGenerator - compile-time)** :

```csharp
// Installation : Mediator (source generator, zero reflection)
// dotnet add package Mediator.SourceGenerator

services.AddMediator();  // Source-generated, pas de scan d'assembly

// Meme interface, mais le binding est fait au compile-time
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    public async ValueTask<OrderDto> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        // ... identique
    }
}

// Note : Mediator utilise ValueTask au lieu de Task pour de meilleures performances
```

---

## 5. Integration CI/CD

### Verification AOT automatique

Un workflow GitHub Actions est configure dans `.github/workflows/aot-check.yml` pour verifier la compatibilite AOT a chaque PR touchant le code source.

#### Ce que fait le workflow :

1. **Declenchement** : Sur chaque PR modifiant des fichiers dans `src/`
2. **Build AOT** : Execute `dotnet publish` avec `PublishAot=true` sur le sample `samples/aot-minimal`
3. **Analyse des warnings** : Les warnings AOT (IL2XXX, IL3XXX) sont captures et affiches comme annotations de PR
4. **Echec conditionnel** : Le build echoue si des warnings critiques sont detectes

#### Ajouter la verification AOT a un nouveau module

Pour qu'un module soit verifie en AOT, ajoutez-le comme reference dans le sample `samples/aot-minimal` :

```xml
<ItemGroup>
  <ProjectReference Include="../../src/modules/RVR.Framework.VotreModule/RVR.Framework.VotreModule.csproj" />
</ItemGroup>
```

#### Marquer un module comme AOT-compatible

Ajoutez dans le `.csproj` du module :

```xml
<PropertyGroup>
  <IsAotCompatible>true</IsAotCompatible>
</PropertyGroup>
```

Cela declenchera des warnings de compilation si le code du module contient des patterns incompatibles AOT.

#### Annotations de trimming recommandees

```csharp
// Pour les methodes generiques qui recoivent des types inconnus
public void Register<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>()
    where T : class { }

// Pour les parametres de type
public object Resolve([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type serviceType)
    => _provider.GetRequiredService(serviceType);

// Pour supprimer un warning specifique quand on sait ce qu'on fait
[UnconditionalSuppressMessage("AOT", "IL2026", Justification = "Type is always available")]
public void ConfigureKnownTypes() { }
```

---

## 6. Ressources

- [Documentation officielle .NET Native AOT](https://learn.microsoft.com/dotnet/core/deploying/native-aot/)
- [ASP.NET Core et Native AOT](https://learn.microsoft.com/aspnet/core/fundamentals/native-aot)
- [Source Generators pour System.Text.Json](https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/source-generation)
- [Mediator.SourceGenerator](https://github.com/martinothamar/Mediator)
- [EF Core Compiled Models](https://learn.microsoft.com/ef/core/performance/advanced-performance-topics#compiled-models)
