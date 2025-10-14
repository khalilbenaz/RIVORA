# CQRS & MediatR

## Principle

RIVORA Framework separates read operations (Queries) from write operations (Commands) via MediatR.

## Commands

Commands modify system state:

```csharp
public record CreateOrderCommand(string CustomerId, List<OrderItemDto> Items)
    : IRequest<OrderDto>;
```

## Queries

Queries read without modifying:

```csharp
[Cached(Duration = 300)]
public record GetProductsQuery : IRequest<List<ProductDto>>;
```

## Pipeline Behaviors

The MediatR pipeline executes in order:

1. **LoggingBehavior**: traces all requests
2. **ValidationBehavior**: automatic FluentValidation
3. **AuthorizationBehavior**: permission checks
4. **CachingBehavior**: caches queries (via `[Cached]` attribute)
5. **TransactionBehavior**: wraps commands in a transaction
6. **PerformanceBehavior**: logs slow requests (> 500ms)

## Domain Events

Domain events are dispatched after `SaveChanges`:

```csharp
public class OrderCreatedHandler : INotificationHandler<OrderCreatedEvent>
{
    public async Task Handle(OrderCreatedEvent evt, CancellationToken ct)
    {
        // Send email, webhook, notification...
    }
}
```
