namespace RVR.Framework.Saga.Models;

/// <summary>
/// Represents the persistent state of a saga instance.
/// </summary>
/// <typeparam name="TData">The type of custom data tracked by the saga.</typeparam>
public class SagaState<TData> where TData : class, new()
{
    /// <summary>
    /// Gets or sets the unique identifier of this saga instance.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the name of the current step in the saga workflow.
    /// </summary>
    public string CurrentStep { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the custom data associated with this saga instance.
    /// </summary>
    public TData Data { get; set; } = new();

    /// <summary>
    /// Gets or sets the current status of the saga.
    /// </summary>
    public SagaStatus Status { get; set; } = SagaStatus.Pending;

    /// <summary>
    /// Gets or sets the UTC timestamp when this saga was started.
    /// </summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the UTC timestamp when this saga completed (null if still running).
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the error message if the saga has failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Represents the possible statuses of a saga.
/// </summary>
public enum SagaStatus
{
    /// <summary>Saga has been created but not yet started.</summary>
    Pending,
    /// <summary>Saga is actively processing steps.</summary>
    InProgress,
    /// <summary>Saga has completed all steps successfully.</summary>
    Completed,
    /// <summary>Saga has failed and may require compensation.</summary>
    Failed,
    /// <summary>Saga compensation steps are being executed.</summary>
    Compensating,
    /// <summary>Saga has been compensated (rolled back).</summary>
    Compensated
}
