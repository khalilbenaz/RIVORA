# Clean Architecture

## Principes

RIVORA Framework applique les principes de Clean Architecture de Robert C. Martin :

- **Independance des frameworks** : le domaine metier ne depend d'aucun framework
- **Testabilite** : la logique metier se teste sans UI, DB, ou serveur web
- **Independance de la UI** : l'API REST peut etre remplacee par gRPC ou GraphQL
- **Independance de la DB** : 4 providers supportes sans changer le domaine

## Couches

### Domain (centre)

Contient les entites, value objects, domain events et specifications. Zero dependance externe.

```csharp
public class Product : AggregateRoot
{
    public string Name { get; private set; }
    public Money Price { get; private set; }
    public ProductStatus Status { get; private set; }

    public void Activate()
    {
        Status = ProductStatus.Active;
        AddDomainEvent(new ProductActivatedEvent(Id));
    }
}
```

### Application

Orchestre les cas d'utilisation via CQRS (MediatR) :

```csharp
public class CreateProductCommand : IRequest<ProductDto>
{
    public string Name { get; init; } = default!;
    public decimal Price { get; init; }
}

public class CreateProductHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductRepository _repo;
    private readonly IMapper _mapper;

    public CreateProductHandler(IProductRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(CreateProductCommand cmd, CancellationToken ct)
    {
        var product = new Product(cmd.Name, cmd.Price);
        await _repo.AddAsync(product, ct);
        return _mapper.Map<ProductDto>(product);
    }
}
```

### Infrastructure

Implemente les interfaces definies dans Domain/Application :

```csharp
public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Product?> GetByNameAsync(string name, CancellationToken ct)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Name == name, ct);
    }
}
```

### Presentation

Controllers minces qui delegent a MediatR :

```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(CreateProductCommand command)
        => Ok(await _mediator.Send(command));

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> Get(Guid id)
        => Ok(await _mediator.Send(new GetProductQuery(id)));
}
```

## Validation

FluentValidation integre au pipeline MediatR :

```csharp
public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}
```

Les erreurs de validation retournent automatiquement un `400 Bad Request` avec les details.
