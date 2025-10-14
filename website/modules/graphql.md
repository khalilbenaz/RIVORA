# Module GraphQL

**Package** : `RVR.Framework.GraphQL`

## Description

Gateway GraphQL base sur HotChocolate 14.3 avec filtering, sorting, projections et subscriptions.

## Enregistrement

```csharp
builder.Services.AddRvrGraphQL(options =>
{
    options.EnableFiltering = true;
    options.EnableSorting = true;
    options.EnableProjections = true;
    options.EnableSubscriptions = true;
    options.MaxDepth = 10;
    options.MaxComplexity = 200;
});

app.MapGraphQL();
```

## Endpoints

| URL | Description |
|-----|-------------|
| `/graphql` | Endpoint GraphQL |
| `/graphql/ui` | Banana Cake Pop (explorateur interactif) |

## Features

- Filtering automatique via `[UseFiltering]`
- Sorting automatique via `[UseSorting]`
- Projections (selection de champs) via `[UseProjection]`
- Subscriptions temps reel via WebSocket
- Pagination relay (cursor-based)
- DataLoader pour eviter le probleme N+1

Voir le [guide GraphQL](/guide/graphql) pour les exemples de resolvers.
