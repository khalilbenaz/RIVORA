# GraphQL

## Configuration

RIVORA Framework integre HotChocolate 14.3 pour un gateway GraphQL complet.

```csharp
builder.Services.AddRvrGraphQL(options =>
{
    options.EnableFiltering = true;
    options.EnableSorting = true;
    options.EnableProjections = true;
    options.EnableIntrospection = true; // Desactiver en production
});
```

## Schema

```graphql
type Query {
  products(where: ProductFilterInput, order: [ProductSortInput!]): [Product!]!
  product(id: UUID!): Product
  orders(where: OrderFilterInput, first: Int, after: String): OrderConnection!
}

type Mutation {
  createProduct(input: CreateProductInput!): Product!
  updateProduct(id: UUID!, input: UpdateProductInput!): Product!
}

type Subscription {
  onOrderCreated: Order!
}
```

## Resolvers

```csharp
[QueryType]
public class ProductQueries
{
    [UseFiltering]
    [UseSorting]
    [UseProjection]
    public IQueryable<Product> GetProducts(ApplicationDbContext context)
        => context.Products;

    public async Task<Product?> GetProduct(Guid id, IProductRepository repo)
        => await repo.GetByIdAsync(id);
}
```

## Client

```graphql
query {
  products(where: { price: { gt: 100 } }, order: { name: ASC }) {
    id
    name
    price
    category { name }
  }
}
```

Endpoint : `http://localhost:5220/graphql`

Banana Cake Pop UI : `http://localhost:5220/graphql/ui`
