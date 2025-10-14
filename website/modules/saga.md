# Saga / Process Manager

The RIVORA Framework includes a saga orchestrator for coordinating long-running, multi-step business processes with compensation (rollback) logic.

## Core Abstractions

### ISaga\<TData\>

Every saga implements `ISaga<TData>` where `TData` holds the saga state:

```csharp
public interface ISaga<TData> where TData : class, new()
{
    Guid SagaId { get; }
    TData Data { get; }
    SagaStatus Status { get; }
    void ConfigureSteps(ISagaStepBuilder<TData> builder);
}
```

### SagaOrchestrator

The orchestrator executes saga steps in sequence and runs compensation handlers if any step fails:

```csharp
public interface ISagaOrchestrator
{
    Task<SagaResult> ExecuteAsync<TData>(ISaga<TData> saga, CancellationToken ct = default)
        where TData : class, new();
    Task<SagaResult> CompensateAsync(Guid sagaId, CancellationToken ct = default);
}
```

## Implementing a Saga

### Step 1: Define the saga data

```csharp
public class OrderFulfillmentData
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentIntentId { get; set; } = string.Empty;
    public string ShipmentTrackingId { get; set; } = string.Empty;
    public bool InventoryReserved { get; set; }
    public bool PaymentCharged { get; set; }
    public bool ShipmentCreated { get; set; }
}
```

### Step 2: Implement the saga

```csharp
public class OrderFulfillmentSaga : ISaga<OrderFulfillmentData>
{
    private readonly IInventoryService _inventory;
    private readonly IPaymentService _payment;
    private readonly IShippingService _shipping;

    public Guid SagaId { get; } = Guid.NewGuid();
    public OrderFulfillmentData Data { get; } = new();
    public SagaStatus Status { get; private set; }

    public OrderFulfillmentSaga(
        IInventoryService inventory,
        IPaymentService payment,
        IShippingService shipping)
    {
        _inventory = inventory;
        _payment = payment;
        _shipping = shipping;
    }

    public void ConfigureSteps(ISagaStepBuilder<OrderFulfillmentData> builder)
    {
        builder
            .Step("ReserveInventory")
            .Execute(async (data, ct) =>
            {
                await _inventory.ReserveAsync(data.OrderId, ct);
                data.InventoryReserved = true;
            })
            .Compensate(async (data, ct) =>
            {
                if (data.InventoryReserved)
                    await _inventory.ReleaseReservationAsync(data.OrderId, ct);
            });

        builder
            .Step("ChargePayment")
            .Execute(async (data, ct) =>
            {
                data.PaymentIntentId = await _payment.ChargeAsync(
                    data.CustomerId, data.TotalAmount, ct);
                data.PaymentCharged = true;
            })
            .Compensate(async (data, ct) =>
            {
                if (data.PaymentCharged)
                    await _payment.RefundAsync(data.PaymentIntentId, ct);
            });

        builder
            .Step("CreateShipment")
            .Execute(async (data, ct) =>
            {
                data.ShipmentTrackingId = await _shipping.CreateShipmentAsync(
                    data.OrderId, ct);
                data.ShipmentCreated = true;
            })
            .Compensate(async (data, ct) =>
            {
                if (data.ShipmentCreated)
                    await _shipping.CancelShipmentAsync(data.ShipmentTrackingId, ct);
            });
    }
}
```

### Step 3: Execute the saga

```csharp
public class OrderService
{
    private readonly ISagaOrchestrator _orchestrator;

    public async Task FulfillOrderAsync(Guid orderId, Guid customerId, decimal total, CancellationToken ct)
    {
        var saga = new OrderFulfillmentSaga(_inventory, _payment, _shipping);
        saga.Data.OrderId = orderId;
        saga.Data.CustomerId = customerId;
        saga.Data.TotalAmount = total;

        var result = await _orchestrator.ExecuteAsync(saga, ct);

        if (result.Status == SagaStatus.Completed)
        {
            // All steps succeeded
            _logger.LogInformation("Order {OrderId} fulfilled. Tracking: {TrackingId}",
                orderId, saga.Data.ShipmentTrackingId);
        }
        else if (result.Status == SagaStatus.Compensated)
        {
            // A step failed; all prior steps were compensated
            _logger.LogWarning("Order {OrderId} fulfillment failed at step '{Step}'. Compensated.",
                orderId, result.FailedStep);
        }
    }
}
```

## Registration

```csharp
builder.Services.AddRvrSagas(options =>
{
    options.PersistSagaState = true;  // Store saga state in DB for recovery
    options.MaxRetries = 3;
    options.RetryDelay = TimeSpan.FromSeconds(5);
});
```

## Saga Lifecycle

```
[Start] --> Step1.Execute --> Step2.Execute --> Step3.Execute --> [Completed]
                                   |
                              (failure)
                                   |
                          Step2.Compensate --> Step1.Compensate --> [Compensated]
```

Compensation runs in reverse order, ensuring consistent rollback.
