# Module Core

**Package** : `RVR.Framework.Core`

## Description

Le module Core fournit les abstractions fondamentales utilisees par tous les autres modules.

## Composants

### Result Pattern

```csharp
public Result<ProductDto> CreateProduct(CreateProductCommand cmd)
{
    if (string.IsNullOrEmpty(cmd.Name))
        return Result<ProductDto>.Failure("Name is required");

    var product = new Product(cmd.Name, cmd.Price);
    return Result<ProductDto>.Success(product.ToDto());
}
```

### Pagination

```csharp
public class PagedResult<T>
{
    public List<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
```

### Specifications

```csharp
public class ActiveProductsSpec : Specification<Product>
{
    public ActiveProductsSpec(decimal? minPrice = null)
    {
        Where(p => p.Status == ProductStatus.Active);
        if (minPrice.HasValue)
            Where(p => p.Price >= minPrice.Value);
        OrderBy(p => p.Name);
    }
}

var products = await _repo.GetAsync(new ActiveProductsSpec(minPrice: 50));
```

### Base Entity

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
```

### Domain Events

```csharp
public abstract class AggregateRoot : BaseEntity
{
    private readonly List<DomainEvent> _domainEvents = [];
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(DomainEvent evt) => _domainEvents.Add(evt);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

## Enregistrement

```csharp
builder.Services.AddRvrCore();
```
