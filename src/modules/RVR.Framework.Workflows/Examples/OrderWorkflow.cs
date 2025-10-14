namespace RVR.Framework.Workflows.Examples;

using RVR.Framework.Workflows.Abstractions;
using RVR.Framework.Workflows.Models;

/// <summary>
/// Possible states for an order workflow.
/// </summary>
public enum OrderState
{
    /// <summary>Order has been created.</summary>
    Created,

    /// <summary>Order has been paid.</summary>
    Paid,

    /// <summary>Order has been shipped.</summary>
    Shipped,

    /// <summary>Order has been delivered.</summary>
    Delivered,

    /// <summary>Order has been cancelled.</summary>
    Cancelled
}

/// <summary>
/// Example workflow definition for a typical e-commerce order lifecycle.
/// States: Created -> Paid -> Shipped -> Delivered, with cancellation allowed from Created or Paid.
/// </summary>
public class OrderWorkflow : IWorkflow<OrderState>
{
    /// <inheritdoc />
    public string Name => "OrderWorkflow";

    /// <inheritdoc />
    public IReadOnlyList<OrderState> States { get; } =
    [
        OrderState.Created,
        OrderState.Paid,
        OrderState.Shipped,
        OrderState.Delivered,
        OrderState.Cancelled
    ];

    /// <inheritdoc />
    public IReadOnlyList<WorkflowTransition<OrderState>> Transitions { get; } =
    [
        new WorkflowTransition<OrderState>
        {
            FromState = OrderState.Created,
            ToState = OrderState.Paid,
            Trigger = "Pay"
        },
        new WorkflowTransition<OrderState>
        {
            FromState = OrderState.Paid,
            ToState = OrderState.Shipped,
            Trigger = "Ship"
        },
        new WorkflowTransition<OrderState>
        {
            FromState = OrderState.Shipped,
            ToState = OrderState.Delivered,
            Trigger = "Deliver"
        },
        new WorkflowTransition<OrderState>
        {
            FromState = OrderState.Created,
            ToState = OrderState.Cancelled,
            Trigger = "Cancel"
        },
        new WorkflowTransition<OrderState>
        {
            FromState = OrderState.Paid,
            ToState = OrderState.Cancelled,
            Trigger = "Cancel"
        }
    ];

    /// <inheritdoc />
    public OrderState InitialState => OrderState.Created;
}
