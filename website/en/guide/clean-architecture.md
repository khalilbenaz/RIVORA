# Clean Architecture

## Principles

RIVORA Framework applies Robert C. Martin's Clean Architecture principles:

- **Framework independence**: business domain depends on no framework
- **Testability**: business logic is testable without UI, DB, or web server
- **UI independence**: REST API can be replaced by gRPC or GraphQL
- **DB independence**: 4 providers supported without changing the domain

## Layers

### Domain (center)

Contains entities, value objects, domain events and specifications. Zero external dependencies.

```csharp
public class Product : AggregateRoot
{
    public string Name { get; private set; }
    public Money Price { get; private set; }

    public void Activate()
    {
        Status = ProductStatus.Active;
        AddDomainEvent(new ProductActivatedEvent(Id));
    }
}
```

### Application

Orchestrates use cases via CQRS (MediatR):

```csharp
public class CreateProductCommand : IRequest<ProductDto>
{
    public string Name { get; init; } = default!;
    public decimal Price { get; init; }
}
```

### Infrastructure

Implements interfaces defined in Domain/Application.

### Presentation

Thin controllers that delegate to MediatR:

```csharp
[HttpPost]
public async Task<ActionResult<ProductDto>> Create(CreateProductCommand command)
    => Ok(await _mediator.Send(command));
```
