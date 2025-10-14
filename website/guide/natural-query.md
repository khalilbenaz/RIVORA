# NL Query (Langage Naturel)

## Principe

Le module NaturalQuery transforme des phrases en francais ou anglais en requetes LINQ executables.

## Configuration

```csharp
builder.Services.AddRvrNaturalQuery(options =>
{
    options.SupportedLanguages = ["fr", "en"];
    options.DefaultLanguage = "fr";
    options.EnableCaching = true;
});
```

## Utilisation

```csharp
// Francais
var results = await _nlQuery.QueryAsync<Product>(
    "produits actifs prix superieur a 100 tries par nom decroissant"
);

// Anglais
var results = await _nlQuery.QueryAsync<Product>(
    "active products with price above 100 sorted by name descending"
);
```

## Operateurs supportes

| Francais | Anglais | LINQ |
|----------|---------|------|
| superieur a, plus de | above, greater than | `>` |
| inferieur a, moins de | below, less than | `<` |
| egal a | equal to | `==` |
| contient | contains | `.Contains()` |
| commence par | starts with | `.StartsWith()` |
| trie par | sorted by, order by | `.OrderBy()` |
| limite a | limit to | `.Take()` |

## API REST

```bash
curl "http://localhost:5220/api/v1/natural-query?entity=Product&q=produits%20actifs%20prix%20%3E%20100" \
  -H "Authorization: Bearer <token>"
```
