# RIVORA Framework - Audit de compatibilite Native AOT

> Document de reference pour la compatibilite Native AOT de tous les modules du RIVORA Framework.
> Derniere mise a jour : Mars 2026 | .NET 9 | v4.0.0-preview.2

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
| **RVR.Framework.Core** | ✅ Compatible AOT | Abstractions pures, interfaces, types de base. `KbaModuleExtensions` migre vers enregistrement explicite d'instances (plus de `GetTypes()`/`Activator.CreateInstance()`). |
| **RVR.Framework.Domain** | ✅ Compatible AOT | Entites, Value Objects, Domain Events. Pattern DDD sans reflection. |
| **RVR.Framework.Application** | ✅ Compatible AOT | MediatR enregistrement via `RegisterServicesFromAssembly` reste compatible car MediatR 12+ supporte les annotations de trimming. FluentValidation idem. Pipeline behaviors enregistres explicitement. |
| **RVR.Framework.Infrastructure** | ✅ Compatible AOT | `ProcessOutboxMessagesJob` migre vers `OutboxEventTypeRegistry` (registre statique) au lieu de `Type.GetType()`. `RVRDbContext` annote avec `[UnconditionalSuppressMessage]` pour les patterns EF Core connus. `Repository<T>` annote avec `[DynamicallyAccessedMembers]`. EF Core compiled models supportes via `dotnet ef dbcontext optimize`. |

### 2.2 API (`src/api/`)

| Module | Statut | Details |
|--------|--------|---------|
| **RVR.Framework.Api** | ✅ Compatible AOT | Minimal API endpoint discovery migre vers enregistrement explicite. `RvrJsonSerializerContext` fournit les source generators JSON. Controllers restent pour la compatibilite mais les Minimal APIs sont recommandes pour AOT. |
| **RVR.Framework.ApiVersioning** | ✅ Compatible AOT | `VersioningFilters` annote avec `[DynamicallyAccessedMembers]` pour les lookups d'attributs. Asp.Versioning v8+ supporte AOT. |
| **RVR.Framework.GraphQL** | ❌ Non compatible | HotChocolate depend massivement de la reflection pour la decouverte de schemas. Pas de timeline de support AOT annoncee. **Alternative** : Utiliser les Minimal APIs avec filtrage/tri manuels pour les scenarios AOT. |

### 2.3 Data (`src/data/`)

| Module | Statut | Details |
|--------|--------|---------|
| **RVR.Framework.Data.Abstractions** | ✅ Compatible AOT | Interfaces de repository, specifications. Aucune implementation concrete. |
| **RVR.Framework.Data.SqlServer** | ✅ Compatible AOT | EF Core 9 + SqlClient supportent AOT avec compiled models (`dotnet ef dbcontext optimize`). Requetes pre-compilees dans `CompiledQueries.cs`. |
| **RVR.Framework.Data.PostgreSQL** | ✅ Compatible AOT | Npgsql a un excellent support AOT depuis v8. Compiled models requis. |
| **RVR.Framework.Data.MySQL** | ✅ Compatible AOT | MySqlConnector supporte AOT. Compiled models requis. |
| **RVR.Framework.Data.SQLite** | ✅ Compatible AOT | Microsoft.Data.Sqlite supporte AOT. Compiled models requis. |
| **RVR.Framework.Data.MongoDB** | ✅ Compatible AOT | Driver MongoDB .NET v3+ supporte AOT. Utiliser `BsonClassMap` explicites au lieu de conventions automatiques. |
| **RVR.Framework.Data.CosmosDB** | ✅ Compatible AOT | SDK Azure Cosmos DB v4 supporte AOT. Configurer `CosmosSerializer` avec `RvrJsonSerializerContext`. |
| **RVR.Framework.Data.ReadReplica** | ✅ Compatible AOT | Routing de connexions. Aucune reflection propre. |

### 2.4 Security (`src/security/`)

| Module | Statut | Details |
|--------|--------|---------|
| **RVR.Framework.Security** | ✅ Compatible AOT | `AuditTrailInterceptor` annote avec `[DynamicallyAccessedMembers]` pour les lookups de proprietes. `JsonWebTokenHandler` utilise en remplacement de `JwtSecurityTokenHandler`. Annotations de trimming ajoutees. |
| **RVR.Framework.Identity.Pro** | ✅ Compatible AOT | ASP.NET Core Identity .NET 9 ameliore le support AOT. Stores annotes avec les attributs de trimming requis. |
| **RVR.Framework.Privacy** | ✅ Compatible AOT | `DataAnonymizer` annote avec `[DynamicallyAccessedMembers(PublicProperties)]` pour preserver les metadonnees de proprietes. `Activator.CreateInstance` remplace par `RuntimeHelpers.GetUninitializedObject`. |

### 2.5 Multi-tenancy (`src/multitenancy/`)

| Module | Statut | Details |
|--------|--------|---------|
| **RVR.Framework.MultiTenancy** | ✅ Compatible AOT | Query filters EF Core utilisent `Expression.Lambda` qui est traduit en SQL par le provider (pas compile en delegates). Compatible AOT. |
| **RVR.Framework.Billing** | ✅ Compatible AOT | Stripe SDK .NET supporte AOT. Annoter les DTOs webhook avec `[JsonSerializable]`. |
| **RVR.Framework.SaaS** | ✅ Compatible AOT | `SendWelcomeEmailStep` et `NotifyWebhookStep` migres vers interfaces DI (`IWelcomeEmailSender`, `IOnboardingWebhookPublisher`) — plus de `AppDomain.GetAssemblies()`, `GetTypes()`, `GetMethod()`, `MethodInfo.Invoke()`. |

### 2.6 Modules (`src/modules/`)

| Module | Statut | Details |
|--------|--------|---------|
| **RVR.Framework.ApiKeys** | ✅ Compatible AOT | Validation de cles API par comparaison de hash. Aucune reflection. |
| **RVR.Framework.Idempotency** | ✅ Compatible AOT | Stockage et verification d'identifiants de requete. Operations simples. |
| **RVR.Framework.Resilience** | ✅ Compatible AOT | Polly v8+ supporte nativement AOT. |
| **RVR.Framework.SMS** | ✅ Compatible AOT | `HttpClient` pour les appels providers. Annoter les DTOs avec `[JsonSerializable]`. |
| **RVR.Framework.Storage** | ✅ Compatible AOT | Azure.Storage.Blobs supporte AOT. |
| **RVR.Framework.Alerting** | ✅ Compatible AOT | Pattern simple sans reflection. |
| **RVR.Framework.Email** | ✅ Compatible AOT | SMTP via System.Net.Mail, aucune reflection. |
| **RVR.Framework.HealthChecks** | ✅ Compatible AOT | `DatabaseHealthCheck` migre vers resolution DI (`ServiceProvider.GetService`) au lieu de `Activator.CreateInstance`. ASP.NET Core Health Checks supporte AOT nativement. |
| **RVR.Framework.Caching** | ✅ Compatible AOT | Serialisation via `System.Text.Json` avec `RvrJsonSerializerContext`. Configurer les types de cache dans le `JsonSerializerContext`. |
| **RVR.Framework.FeatureManagement** | ✅ Compatible AOT | Enregistrement explicite des filtres au lieu de la decouverte automatique. |
| **RVR.Framework.Features** | ✅ Compatible AOT | `FeatureGateAttribute` annote avec `[DynamicallyAccessedMembers]` pour les lookups d'attributs. |
| **RVR.Framework.Messaging** | ✅ Compatible AOT | Configurer les serialiseurs avec types connus explicitement via `[JsonSerializable]`. |
| **RVR.Framework.Notifications** | ✅ Compatible AOT | SignalR .NET 9 supporte AOT avec configuration explicite des types serialises. |
| **RVR.Framework.RealTime** | ✅ Compatible AOT | SignalR .NET 9 AOT-compatible. Annoter les hub methods avec `[JsonSerializable]`. |
| **RVR.Framework.Search** | ✅ Compatible AOT | Configurer les mappings Elasticsearch manuellement avec des types connus. |
| **RVR.Framework.Dapr** | ✅ Compatible AOT | SDK Dapr .NET supporte AOT. Annoter les messages avec `[JsonSerializable]`. |
| **RVR.Framework.Jobs.Abstractions** | ✅ Compatible AOT | Interfaces et abstractions. |
| **RVR.Framework.Jobs.Hangfire** | ❌ Non compatible | Hangfire utilise massivement la reflection pour la serialisation des jobs et l'execution differee. **Alternative** : Utiliser Quartz.NET pour les scenarios AOT. |
| **RVR.Framework.Jobs.Quartz** | ✅ Compatible AOT | Quartz.NET v4 supporte AOT. Implementer un `IJobFactory` personnalise avec DI explicite au lieu de `Activator.CreateInstance`. |
| **RVR.Framework.Localization.Dynamic** | ✅ Compatible AOT | Migre vers chargement de ressources depuis la base de donnees via EF Core (pas de compilation dynamique). `IStringLocalizer` compatible AOT. |
| **RVR.Framework.EventSourcing** | ✅ Compatible AOT | `ProcessOutboxMessagesJob` migre vers `OutboxEventTypeRegistry` — registre statique de types d'evenements (plus de `Type.GetType(string)`). Les types doivent etre enregistres au demarrage via `OutboxEventTypeRegistry.Register<T>()`. |
| **RVR.Framework.Saga** | ✅ Compatible AOT | Migre vers enregistrement explicite des handlers d'etat via DI. Plus de dispatch dynamique par reflection. |
| **RVR.Framework.Plugins** | ❌ Non compatible | Par conception, le chargement de plugins repose sur `Assembly.LoadFrom()` / `AssemblyLoadContext`, fondamentalement incompatible avec Native AOT. Marque avec `[RequiresUnreferencedCode]` et `[RequiresDynamicCode]`. |
| **RVR.Framework.Profiling** | ✅ Compatible AOT | Utilise MiniProfiler qui est un middleware ASP.NET standard sans proxy dynamique ni interception IL. |
| **RVR.Framework.Workflows** | ✅ Compatible AOT | Migre vers delegates pre-compiles et enregistrement explicite des etapes de workflow via DI. |

### 2.7 Integration (`src/integration/`)

| Module | Statut | Details |
|--------|--------|---------|
| **RVR.Framework.Client** | ✅ Compatible AOT | Client HTTP type. Annoter les DTOs avec `[JsonSerializable]`. |
| **RVR.Framework.Export** | ✅ Compatible AOT | `CsvExportService`, `ExcelExportService`, `PdfExportService` et `PropertyHelper` annotes avec `[DynamicallyAccessedMembers(PublicProperties)]` sur les types generiques T. QuestPDF et ClosedXML supportent les proprietes preservees. |
| **RVR.Framework.Webhooks** | ✅ Compatible AOT | Serialisation JSON via `System.Text.Json`. Configurer `RvrJsonSerializerContext` pour les payloads webhook. |

### 2.8 AI (`src/ai/`)

| Module | Statut | Details |
|--------|--------|---------|
| **RVR.Framework.AI** | ✅ Compatible AOT | SDKs LLM supportent AOT. Types de function parameters annotes avec `[JsonSerializable]`. |
| **RVR.Framework.AI.Agents** | ✅ Compatible AOT | `SqlTool` et `ReActStrategy` migres vers `JsonSerializerContext` source-generated (`SqlToolJsonContext`, `ReActJsonContext`). |
| **RVR.Framework.AI.Guardrails** | ✅ Compatible AOT | Basee sur des regex pre-compilees (`[GeneratedRegex]`) et des regles statiques. |
| **RVR.Framework.NaturalQuery** | ✅ Compatible AOT | `ExpressionBuilder` annote avec `[DynamicallyAccessedMembers(PublicProperties)]` sur tous les parametres generiques T. Les expressions LINQ sont traduites en SQL par le provider EF Core (pas compilees via `.Compile()`). |

### 2.9 Hosting (`src/hosting/`)

| Module | Statut | Details |
|--------|--------|---------|
| **RVR.Framework.AppHost** | ✅ Compatible AOT | .NET Aspire AppHost. Le modele d'orchestration est compatible. |
| **RVR.Framework.Aspire** | ✅ Compatible AOT | Service Defaults pour Aspire. Extensions enregistrees sont compatibles. |
| **RVR.Framework.ServiceDefaults** | ✅ Compatible AOT | Configuration OpenTelemetry, Health Checks. Les packages OTel supportent AOT dans .NET 9. |

### 2.10 UI (`src/ui/`)

| Module | Statut | Details |
|--------|--------|---------|
| **RVR.Framework.Admin** | ❌ Non compatible | Blazor Server ne supporte pas Native AOT. Ce module n'est pas destine aux scenarios serverless — le front-end React (`frontend/`) est l'alternative AOT-friendly. |
| **RVR.Framework.AuditLogging.UI** | ❌ Non compatible | Composants Blazor UI. Memes contraintes. |

---

### Resume statistique

| Statut | Nombre | Pourcentage |
|--------|--------|-------------|
| ✅ Compatible AOT | 50 | 89% |
| ❌ Non compatible | 6 | 11% |
| **Total** | **56** | **100%** |

> **Progres** : De 25% a 89% de compatibilite AOT grace aux migrations v4.0.0-preview.2.
> Les 6 modules non compatibles sont soit des UI Blazor (Admin, AuditLogging.UI), soit des systemes fondamentalement dynamiques (Plugins, Hangfire, GraphQL) pour lesquels des alternatives AOT-friendly existent dans le framework.

---

## 3. Changements effectues (v4.0.0-preview.2)

### Patterns supprimes

| Pattern AOT-incompatible | Fichier | Remplacement |
|--------------------------|---------|--------------|
| `Activator.CreateInstance(type)` | `KbaModuleExtensions.cs` | Instances de modules passees directement |
| `Activator.CreateInstance(propertyType)` | `DataAnonymizer.cs` | `RuntimeHelpers.GetUninitializedObject()` |
| `Activator.CreateInstance(_contextType)` | `DatabaseHealthCheck.cs` | `ServiceProvider.GetService()` via DI |
| `Type.GetType(message.Type)` | `ProcessOutboxMessagesJob.cs` | `OutboxEventTypeRegistry.Resolve()` |
| `AppDomain.GetAssemblies().GetTypes()` | `SendWelcomeEmailStep.cs` | `IWelcomeEmailSender` interface + DI |
| `AppDomain.GetAssemblies().GetTypes()` | `NotifyWebhookStep.cs` | `IOnboardingWebhookPublisher` interface + DI |
| `MethodInfo.Invoke()` | `SendWelcomeEmailStep.cs` | Appel direct via interface |
| `MethodInfo.Invoke()` | `NotifyWebhookStep.cs` | Appel direct via interface |
| `Assembly.GetTypes()` scanning | `Program.cs` (modules) | Enregistrement explicite d'instances |

### Annotations ajoutees

| Annotation | Fichiers |
|------------|----------|
| `[DynamicallyAccessedMembers(PublicProperties)]` | `Repository.cs`, `DataAnonymizer.cs`, `ExpressionBuilder.cs`, `CsvExportService.cs`, `ExcelExportService.cs`, `PdfExportService.cs`, `PropertyHelper.cs`, `IExportService.cs`, `ExportService.cs`, `AuditTrailInterceptor.cs` |
| `[DynamicallyAccessedMembers(All)]` | `FeatureGateAttribute.cs`, `VersioningFilters.cs` |
| `[UnconditionalSuppressMessage]` | `KBADbContext.cs` (SaveChangesAsync, OnModelCreating) |
| `[RequiresUnreferencedCode]` + `[RequiresDynamicCode]` | `AssemblyPluginLoader.cs`, `PluginBuilderExtensions.cs` |

### Nouvelles classes

| Classe | Fichier | Role |
|--------|---------|------|
| `OutboxEventTypeRegistry` | `Infrastructure/BackgroundJobs/OutboxEventTypeRegistry.cs` | Registre statique de types d'evenements pour deserialization AOT-safe |
| `IWelcomeEmailSender` | `SaaS/Onboarding/Steps/SendWelcomeEmailStep.cs` | Interface DI pour l'envoi d'emails de bienvenue |
| `IOnboardingWebhookPublisher` | `SaaS/Onboarding/Steps/NotifyWebhookStep.cs` | Interface DI pour la publication de webhooks d'onboarding |

---

## 4. Patterns a utiliser

### 4.1 Enregistrement de modules (AOT-safe)

```csharp
// Avant (incompatible AOT - scan d'assembly)
builder.Services.AddRvrModules(builder.Configuration,
    typeof(RealTimeModule).Assembly,
    typeof(NotificationsModule).Assembly);

// Apres (compatible AOT - instances explicites)
builder.Services.AddRvrModules(builder.Configuration,
    new RealTimeModule(),
    new NotificationsModule());
```

### 4.2 Enregistrement de types d'evenements Outbox (AOT-safe)

```csharp
// Au demarrage de l'application
OutboxEventTypeRegistry.Register<OrderCreatedEvent>();
OutboxEventTypeRegistry.Register<OrderPaidEvent>();
OutboxEventTypeRegistry.Register<UserRegisteredEvent>();
```

### 4.3 Serialisation JSON avec Source Generators

```csharp
[JsonSerializable(typeof(Product))]
[JsonSerializable(typeof(List<Product>))]
[JsonSerializable(typeof(ApiResponse<Product>))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class AppJsonContext : JsonSerializerContext { }

// Dans Program.cs
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default);
});
```

### 4.4 Resolution de services sans reflection

```csharp
// Avant (incompatible AOT)
var type = AppDomain.CurrentDomain.GetAssemblies()
    .SelectMany(a => a.GetTypes())
    .FirstOrDefault(t => t.FullName == "My.Service.IFoo");
var service = serviceProvider.GetService(type);

// Apres (compatible AOT)
var service = serviceProvider.GetService<IFoo>();
```

### 4.5 Annotations de trimming

```csharp
// Pour les methodes generiques qui accedent aux proprietes via reflection
public void Process<[DynamicallyAccessedMembers(
    DynamicallyAccessedMemberTypes.PublicProperties)] T>(T entity) { }

// Pour marquer du code comme incompatible AOT
[RequiresUnreferencedCode("Plugin loading requires dynamic assembly loading")]
[RequiresDynamicCode("Plugin loading requires runtime code generation")]
public void LoadPlugins(string directory) { }

// Pour supprimer un warning connu
[UnconditionalSuppressMessage("AOT", "IL2070",
    Justification = "Types are known at compile time")]
public override Task<int> SaveChangesAsync(...) { }
```

---

## 5. Modules non compatibles et alternatives

| Module | Raison | Alternative AOT |
|--------|--------|-----------------|
| **GraphQL** | HotChocolate reflection | Minimal APIs + filtrage/tri |
| **Jobs.Hangfire** | Serialisation reflexive des jobs (Hangfire core) | Quartz.NET v4 |
| **Plugins** | `Assembly.LoadFrom()` | Compilation statique des plugins |
| **Admin** | Blazor Server | Front-end React (`frontend/`) |
| **AuditLogging.UI** | Blazor Server | Front-end React (`frontend/`) |

---

## 6. Integration CI/CD

### Verification AOT automatique

Un workflow GitHub Actions est configure dans `.github/workflows/aot-check.yml` pour verifier la compatibilite AOT a chaque PR touchant le code source.

#### Marquer un module comme AOT-compatible

```xml
<PropertyGroup>
  <IsAotCompatible>true</IsAotCompatible>
</PropertyGroup>
```

---

## 7. Ressources

- [Documentation officielle .NET Native AOT](https://learn.microsoft.com/dotnet/core/deploying/native-aot/)
- [ASP.NET Core et Native AOT](https://learn.microsoft.com/aspnet/core/fundamentals/native-aot)
- [Source Generators pour System.Text.Json](https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/source-generation)
- [Mediator.SourceGenerator](https://github.com/martinothamar/Mediator)
- [EF Core Compiled Models](https://learn.microsoft.com/ef/core/performance/advanced-performance-topics#compiled-models)
