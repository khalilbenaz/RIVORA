# CQRS & MediatR

## Principe

RIVORA Framework separe les operations de lecture (Queries) et d'ecriture (Commands) via MediatR.

## Commands

Les commands modifient l'etat du systeme :

```csharp
// Definition
public record CreateOrderCommand(string CustomerId, List<OrderItemDto> Items)
    : IRequest<OrderDto>;

// Handler
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOrderRepository _repo;
    private readonly IMediator _mediator;

    public async Task<OrderDto> Handle(CreateOrderCommand cmd, CancellationToken ct)
    {
        var order = Order.Create(cmd.CustomerId, cmd.Items);
        await _repo.AddAsync(order, ct);

        // Domain events dispatches automatiquement
        return order.ToDto();
    }
}
```

## Queries

Les queries lisent sans modifier :

```csharp
public record GetOrdersQuery(string? Status, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<OrderDto>>;

public class GetOrdersHandler : IRequestHandler<GetOrdersQuery, PagedResult<OrderDto>>
{
    private readonly IReadOnlyRepository<Order> _repo;

    public async Task<PagedResult<OrderDto>> Handle(GetOrdersQuery query, CancellationToken ct)
    {
        var spec = new OrdersByStatusSpec(query.Status, query.Page, query.PageSize);
        return await _repo.GetPagedAsync(spec, ct);
    }
}
```

## Pipeline Behaviors

Le pipeline MediatR execute dans l'ordre :

1. **LoggingBehavior** : trace toutes les requetes
2. **ValidationBehavior** : FluentValidation automatique
3. **AuthorizationBehavior** : verifie les permissions
4. **CachingBehavior** : cache les queries (attribut `[Cached]`)
5. **TransactionBehavior** : wraps les commands dans une transaction
6. **PerformanceBehavior** : log les requetes lentes (> 500ms)

```csharp
// Activer le cache sur une query
[Cached(Duration = 300)] // 5 minutes
public record GetProductsQuery : IRequest<List<ProductDto>>;
```

## Domain Events

Les domain events sont dispatches apres le `SaveChanges` :

```csharp
public class OrderCreatedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public string CustomerId { get; }
}

public class OrderCreatedHandler : INotificationHandler<OrderCreatedEvent>
{
    public async Task Handle(OrderCreatedEvent evt, CancellationToken ct)
    {
        // Envoyer email, webhook, notification...
    }
}
```
