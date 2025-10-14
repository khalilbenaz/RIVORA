# GraphQL Module

**Package**: `RVR.Framework.GraphQL`

GraphQL gateway based on HotChocolate 14.3 with filtering, sorting, projections and subscriptions.

```csharp
builder.Services.AddRvrGraphQL(options =>
{
    options.EnableFiltering = true;
    options.EnableSorting = true;
    options.EnableProjections = true;
});

app.MapGraphQL();
```

See the [French documentation](/modules/graphql) for detailed API reference.
