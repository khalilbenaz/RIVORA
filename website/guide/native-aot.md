# Native AOT

La compilation Native AOT (Ahead-of-Time) permet de compiler votre application RIVORA en code natif, eliminant le besoin du runtime .NET au demarrage. Cela reduit drastiquement le temps de demarrage et l'empreinte memoire.

## Qu'est-ce que Native AOT ?

Avec .NET 9, la compilation AOT transforme votre code C# directement en code machine natif au moment de la compilation, au lieu de le compiler en IL (Intermediate Language) qui serait ensuite interprete par le JIT au runtime.

### Avantages

| Metrique | JIT classique | Native AOT |
|----------|--------------|------------|
| Temps de demarrage | ~800ms | ~80ms |
| Memoire au demarrage | ~120 MB | ~30 MB |
| Taille du binaire | ~5 MB + runtime | ~15 MB autonome |
| Premiere requete | ~200ms | ~5ms |

### Cas d'utilisation ideaux

- **Fonctions serverless** (Azure Functions, AWS Lambda) ou le cold start est critique
- **Microservices** avec demarrage rapide et faible empreinte
- **Conteneurs** ou la taille de l'image compte
- **Applications CLI** qui doivent demarrer instantanement

## Compatibilite RIVORA

RIVORA vise une compatibilite AOT de **89%** sur l'ensemble de ses modules. Voici l'etat actuel :

| Module | Compatible AOT | Notes |
|--------|---------------|-------|
| Core | Oui | Entierement compatible |
| Security | Oui | JWT + HMAC compatibles |
| Caching (Redis) | Oui | Utilise `System.Text.Json` |
| Jobs (Hangfire) | Non | Necessite reflexion |
| Export PDF | Partiel | QuestPDF compatible, iText non |
| GraphQL (HotChocolate) | Partiel | Support experimental |
| EF Core | Oui | Depuis EF Core 9 |
| MediatR | Oui | Avec source generators |
| SignalR | Oui | Depuis .NET 9 |
| AI / Semantic Kernel | Partiel | En progression |

## Activer Native AOT

### 1. Configurer le projet

Ajoutez dans votre fichier `.csproj` :

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <PublishAot>true</PublishAot>
    <InvariantGlobalization>false</InvariantGlobalization>
    <StripSymbols>true</StripSymbols>
  </PropertyGroup>
</Project>
```

### 2. Publier en AOT

```bash
dotnet publish -c Release -r linux-x64

# Ou via la CLI RIVORA
rvr publish --target self-contained --aot
```

### 3. Verifier les avertissements

Executez l'analyse de trimming pour detecter les problemes :

```bash
dotnet publish -c Release -r linux-x64 /p:PublishAot=true /p:TrimmerSingleWarn=false
```

Les avertissements `IL2XXX` et `IL3XXX` indiquent du code incompatible.

## Marquer un module comme AOT-compatible

Pour declarer un module compatible AOT, ajoutez les attributs suivants :

```csharp
// Dans AssemblyInfo.cs du module
[assembly: System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("false")]

// Ou dans le .csproj
// <IsAotCompatible>true</IsAotCompatible>
```

Dans le code du module, utilisez les annotations de trimming :

```csharp
using System.Diagnostics.CodeAnalysis;

public class ProductService
{
    // Indique que ce type est preserve pour AOT
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
    public Type EntityType => typeof(Product);

    // Methode compatible AOT avec source generator JSON
    public string Serialize(Product product)
    {
        return JsonSerializer.Serialize(product, ProductJsonContext.Default.Product);
    }
}

// Source generator pour System.Text.Json (obligatoire en AOT)
[JsonSerializable(typeof(Product))]
[JsonSerializable(typeof(List<Product>))]
[JsonSerializable(typeof(PagedResult<Product>))]
public partial class ProductJsonContext : JsonSerializerContext { }
```

## Patterns a eviter

Ces patterns utilisent la reflexion et ne fonctionnent pas avec AOT :

### Reflexion dynamique

```csharp
// INTERDIT en AOT
var type = Type.GetType("MyApp.Services.ProductService");
var instance = Activator.CreateInstance(type);

// INTERDIT - reflexion sur les proprietes
var props = typeof(Product).GetProperties();
foreach (var prop in props)
{
    var value = prop.GetValue(entity);
}
```

### Serialisation sans source generator

```csharp
// INTERDIT en AOT
var json = JsonSerializer.Serialize(product);
var obj = JsonSerializer.Deserialize<Product>(json);

// INTERDIT - Newtonsoft.Json
var json = JsonConvert.SerializeObject(product);
```

### Dynamic et ExpandoObject

```csharp
// INTERDIT en AOT
dynamic obj = new ExpandoObject();
obj.Name = "Test";

// INTERDIT
dynamic result = someService.GetResult();
```

## Patterns recommandes

### Source generators pour JSON

```csharp
// CORRECT - Utiliser les source generators
[JsonSerializable(typeof(ApiResponse<Product>))]
[JsonSerializable(typeof(ApiResponse<List<Product>>))]
[JsonSerializable(typeof(ErrorResponse))]
public partial class AppJsonContext : JsonSerializerContext { }

// Registration dans Program.cs
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default);
});
```

### Injection de dependances au lieu de la reflexion

```csharp
// CORRECT - DI avec interfaces
public interface IEntityMapper<TEntity, TDto>
{
    TDto Map(TEntity entity);
}

// Registration explicite
builder.Services.AddSingleton<IEntityMapper<Product, ProductDto>, ProductMapper>();
```

### Generic math et interfaces statiques

```csharp
// CORRECT - Compile-time polymorphism
public interface IValueObject<TSelf> where TSelf : IValueObject<TSelf>
{
    static abstract TSelf Create(string value);
    static abstract bool IsValid(string value);
}

public readonly record struct Email : IValueObject<Email>
{
    public string Value { get; }
    public static Email Create(string value) => new(value);
    public static bool IsValid(string value) => value.Contains('@');
}
```

## Docker avec AOT

Optimisez vos images Docker avec AOT :

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/RVR.Framework.Api/RVR.Framework.Api.csproj \
    -c Release -r linux-x64 -o /app/publish

# Runtime stage - pas besoin du runtime .NET !
FROM mcr.microsoft.com/dotnet/runtime-deps:9.0-noble-chiseled
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["./RVR.Framework.Api"]
```

Comparaison des tailles d'image :

| Image | Taille |
|-------|--------|
| SDK + runtime classique | ~800 MB |
| Runtime classique (chiseled) | ~120 MB |
| AOT (runtime-deps chiseled) | ~45 MB |

## Diagnostiquer les problemes AOT

### Outil d'analyse

```bash
# Analyser la compatibilite AOT
dotnet publish -c Release -r linux-x64 /p:PublishAot=true 2>&1 | grep "warning IL"

# Via la CLI RIVORA
rvr ai review --architecture --aot-compat
```

### Avertissements courants

| Code | Description | Solution |
|------|-------------|----------|
| IL2026 | RequiresUnreferencedCode | Ajouter des annotations ou utiliser des source generators |
| IL2057 | Type.GetType dynamique | Utiliser des references de type directes |
| IL2070 | Reflexion sur les membres | Utiliser `DynamicallyAccessedMembers` |
| IL3050 | RequiresDynamicCode | Eviter les generics ouverts au runtime |

## Etape suivante

- [CI/CD](/guide/ci-cd) pour integrer AOT dans votre pipeline
- [Docker](/guide/docker) pour optimiser vos images conteneurs
- [Monitoring](/guide/monitoring) pour mesurer les gains de performance
