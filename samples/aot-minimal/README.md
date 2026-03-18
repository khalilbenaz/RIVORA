# RVR.AOT.Minimal - Sample Native AOT RIVORA API

Ce sample demontre une API minimale RIVORA publiee en Native AOT.

## Prerequis

- .NET 9 SDK
- Outils de compilation native pour votre plateforme :
  - **Windows** : Visual Studio 2022 avec "Desktop development with C++" workload
  - **Linux** : `sudo apt install clang zlib1g-dev`
  - **macOS** : Xcode Command Line Tools (`xcode-select --install`)

## Build standard (JIT)

```bash
dotnet run
```

## Publication Native AOT

```bash
# Publication pour la plateforme courante
dotnet publish -c Release

# Publication pour une plateforme specifique
dotnet publish -c Release -r linux-x64
dotnet publish -c Release -r win-x64
dotnet publish -c Release -r osx-arm64
```

Le binaire natif se trouve dans `bin/Release/net9.0/<rid>/publish/`.

## Tester

```bash
# Health check
curl http://localhost:5000/health

# Lister les produits
curl http://localhost:5000/api/products

# Creer un produit
curl -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -d '{"name": "Mon Produit", "price": 49.99, "category": "Test"}'

# Mettre a jour un produit
curl -X PUT http://localhost:5000/api/products/{id} \
  -H "Content-Type: application/json" \
  -d '{"name": "Produit Mis a Jour"}'

# Supprimer un produit
curl -X DELETE http://localhost:5000/api/products/{id}
```

## Modules RIVORA references

Ce sample reference uniquement les modules confirmes compatibles AOT :

- `RVR.Framework.Core` - Abstractions de base
- `RVR.Framework.Domain` - Entites et Value Objects
- `RVR.Framework.ApiKeys` - Authentification par cle API

## Points cles AOT

1. **`[JsonSerializable]`** : Tous les types serialises sont declares dans `AppJsonContext`
2. **Pas de reflection** : Aucun `Activator.CreateInstance`, `Type.GetType`, ou `Assembly.Load`
3. **Pas de MediatR** : Les endpoints appellent directement la logique metier
4. **Pas d'EF Core** : Stockage en memoire avec `ConcurrentDictionary`
5. **`WebApplication.CreateSlimBuilder`** : Version allegee du builder, optimisee pour AOT
