namespace RVR.Framework.SaaS.Onboarding;

/// <summary>
/// Represents a single step in the tenant onboarding pipeline.
/// Each step supports forward execution and compensating (rollback) logic for the Saga pattern.
/// </summary>
public interface ITenantOnboardingStep
{
    /// <summary>
    /// Gets the display name of this onboarding step.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the execution order of this step within the pipeline.
    /// Steps are executed in ascending order.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Executes the onboarding step.
    /// </summary>
    /// <param name="context">The current onboarding context containing tenant information.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result describing whether the step succeeded or failed.</returns>
    Task<OnboardingStepResult> ExecuteAsync(TenantOnboardingContext context, CancellationToken ct = default);

    /// <summary>
    /// Compensates (rolls back) the effects of a previously executed step.
    /// Called during Saga compensation when a subsequent step fails.
    /// </summary>
    /// <param name="context">The current onboarding context containing tenant information.</param>
    /// <param name="ct">Cancellation token.</param>
    Task CompensateAsync(TenantOnboardingContext context, CancellationToken ct = default);
}

/// <summary>
/// Holds the mutable state carried through the onboarding pipeline.
/// </summary>
public sealed record TenantOnboardingContext
{
    /// <summary>
    /// Gets or sets the unique identifier for the tenant being onboarded.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the display name of the tenant.
    /// </summary>
    public string TenantName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the subscription plan identifier (e.g., "free", "pro", "enterprise").
    /// </summary>
    public string Plan { get; set; } = "free";

    /// <summary>
    /// Gets or sets the email address of the initial admin user.
    /// </summary>
    public string AdminEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional password for the initial admin user.
    /// When null, a random password should be generated and included in the welcome email.
    /// </summary>
    public string? AdminPassword { get; set; }

    /// <summary>
    /// Gets or sets additional custom properties for the onboarding pipeline.
    /// Steps may read from or write to this dictionary to pass data between steps.
    /// </summary>
    public Dictionary<string, object> Properties { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of step results for steps that have completed successfully.
    /// Used during compensation to determine which steps need rollback.
    /// </summary>
    public List<OnboardingStepResult> CompletedSteps { get; set; } = new();
}

/// <summary>
/// Describes the outcome of a single onboarding step execution.
/// </summary>
/// <param name="StepName">The name of the step that produced this result.</param>
/// <param name="Success">Whether the step completed successfully.</param>
/// <param name="Message">An optional human-readable message describing the outcome.</param>
/// <param name="Data">Optional key-value data produced by the step (e.g., generated credentials).</param>
public sealed record OnboardingStepResult(
    string StepName,
    bool Success,
    string? Message = null,
    Dictionary<string, object>? Data = null);
