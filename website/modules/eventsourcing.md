# Event Sourcing

The RIVORA Framework provides event sourcing primitives to persist domain events as the source of truth, enabling full audit trails and temporal queries.

## Core Abstractions

### IAggregateRoot

All event-sourced aggregates implement `IAggregateRoot`:

```csharp
public interface IAggregateRoot
{
    Guid Id { get; }
    int Version { get; }
    IReadOnlyList<IDomainEvent> UncommittedEvents { get; }
    void LoadFromHistory(IEnumerable<IDomainEvent> history);
    void ClearUncommittedEvents();
}
```

### IEventStore

The event store persists and retrieves domain events:

```csharp
public interface IEventStore
{
    Task SaveEventsAsync(Guid aggregateId, IEnumerable<IDomainEvent> events, int expectedVersion, CancellationToken ct = default);
    Task<IReadOnlyList<IDomainEvent>> GetEventsAsync(Guid aggregateId, CancellationToken ct = default);
    Task<IReadOnlyList<IDomainEvent>> GetEventsAsync(Guid aggregateId, int fromVersion, CancellationToken ct = default);
}
```

### InMemoryEventStore

A ready-to-use in-memory implementation for development and testing:

```csharp
builder.Services.AddSingleton<IEventStore, InMemoryEventStore>();
```

## Creating an Event-Sourced Aggregate

### Step 1: Define domain events

```csharp
public record OrderCreated(Guid OrderId, Guid CustomerId, DateTime CreatedAt) : IDomainEvent;
public record OrderItemAdded(Guid OrderId, Guid ProductId, int Quantity, decimal UnitPrice) : IDomainEvent;
public record OrderSubmitted(Guid OrderId, DateTime SubmittedAt) : IDomainEvent;
public record OrderCancelled(Guid OrderId, string Reason, DateTime CancelledAt) : IDomainEvent;
```

### Step 2: Implement the aggregate

```csharp
public class Order : AggregateRoot
{
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public List<OrderItem> Items { get; private set; } = new();
    public decimal TotalAmount => Items.Sum(i => i.Quantity * i.UnitPrice);

    // Constructor for creating a new order
    public Order(Guid customerId)
    {
        RaiseEvent(new OrderCreated(Guid.NewGuid(), customerId, DateTime.UtcNow));
    }

    // Private constructor for rehydration
    private Order() { }

    public void AddItem(Guid productId, int quantity, decimal unitPrice)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Cannot add items to a non-draft order.");

        RaiseEvent(new OrderItemAdded(Id, productId, quantity, unitPrice));
    }

    public void Submit()
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Only draft orders can be submitted.");
        if (!Items.Any())
            throw new InvalidOperationException("Cannot submit an empty order.");

        RaiseEvent(new OrderSubmitted(Id, DateTime.UtcNow));
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Order is already cancelled.");

        RaiseEvent(new OrderCancelled(Id, reason, DateTime.UtcNow));
    }

    // Event handlers (called during rehydration and when events are raised)
    private void Apply(OrderCreated e)
    {
        Id = e.OrderId;
        CustomerId = e.CustomerId;
        Status = OrderStatus.Draft;
    }

    private void Apply(OrderItemAdded e)
    {
        Items.Add(new OrderItem(e.ProductId, e.Quantity, e.UnitPrice));
    }

    private void Apply(OrderSubmitted e)
    {
        Status = OrderStatus.Submitted;
    }

    private void Apply(OrderCancelled e)
    {
        Status = OrderStatus.Cancelled;
    }
}
```

### Step 3: Use the aggregate with the event store

```csharp
public class OrderService
{
    private readonly IEventStore _eventStore;

    public OrderService(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task<Order> CreateOrderAsync(Guid customerId, CancellationToken ct)
    {
        var order = new Order(customerId);
        await _eventStore.SaveEventsAsync(order.Id, order.UncommittedEvents, 0, ct);
        order.ClearUncommittedEvents();
        return order;
    }

    public async Task<Order> GetOrderAsync(Guid orderId, CancellationToken ct)
    {
        var events = await _eventStore.GetEventsAsync(orderId, ct);
        var order = new Order();
        order.LoadFromHistory(events);
        return order;
    }

    public async Task AddItemAsync(Guid orderId, Guid productId, int quantity, decimal unitPrice, CancellationToken ct)
    {
        var order = await GetOrderAsync(orderId, ct);
        order.AddItem(productId, quantity, unitPrice);
        await _eventStore.SaveEventsAsync(orderId, order.UncommittedEvents, order.Version, ct);
        order.ClearUncommittedEvents();
    }
}
```

## Registration

```csharp
// In Program.cs
builder.Services.AddRvrEventSourcing(options =>
{
    options.UseInMemoryStore();       // For development
    // options.UseSqlServerStore();   // For production
});
```

## Replaying Events

Replay events to rebuild read models or projections:

```csharp
var events = await _eventStore.GetEventsAsync(orderId);
var order = new Order();
order.LoadFromHistory(events);

// order is now fully rehydrated to its current state
Console.WriteLine($"Order {order.Id}: {order.Status}, {order.Items.Count} items, total: {order.TotalAmount}");
```
