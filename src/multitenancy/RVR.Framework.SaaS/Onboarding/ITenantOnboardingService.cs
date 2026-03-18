namespace RVR.Framework.SaaS.Onboarding;

/// <summary>
/// Orchestrates tenant onboarding by executing a pipeline of <see cref="ITenantOnboardingStep"/>
/// instances with full Saga-pattern compensation on failure.
/// </summary>
public interface ITenantOnboardingService
{
    /// <summary>
    /// Provisions a new tenant by running every registered onboarding step in order.
    /// If any step fails, previously completed steps are compensated in reverse order.
    /// </summary>
    /// <param name="request">The tenant provisioning request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A detailed result containing the outcome of every step.</returns>
    Task<OnboardingResult> ProvisionTenantAsync(TenantProvisionRequest request, CancellationToken ct = default);

    /// <summary>
    /// Gets the current onboarding status for a tenant.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The current onboarding status including completed steps.</returns>
    Task<OnboardingStatus> GetStatusAsync(Guid tenantId, CancellationToken ct = default);
}

/// <summary>
/// Request model for provisioning a new tenant through the onboarding wizard.
/// </summary>
/// <param name="TenantName">The display name for the new tenant.</param>
/// <param name="Plan">The subscription plan identifier (e.g., "free", "pro", "enterprise").</param>
/// <param name="AdminEmail">The email address of the initial admin user.</param>
/// <param name="AdminPassword">An optional password for the admin user. When null, a password is auto-generated.</param>
/// <param name="Properties">Optional custom properties passed through the onboarding pipeline.</param>
public sealed record TenantProvisionRequest(
    string TenantName,
    string Plan,
    string AdminEmail,
    string? AdminPassword = null,
    Dictionary<string, object>? Properties = null);

/// <summary>
/// The overall result of a tenant onboarding operation.
/// </summary>
/// <param name="Success">Whether all onboarding steps completed successfully.</param>
/// <param name="TenantId">The unique identifier assigned to the new tenant.</param>
/// <param name="Steps">The ordered list of step results.</param>
/// <param name="Error">An optional error message if the onboarding failed.</param>
public sealed record OnboardingResult(
    bool Success,
    Guid TenantId,
    List<OnboardingStepResult> Steps,
    string? Error = null);

/// <summary>
/// Represents the current onboarding status for a tenant.
/// </summary>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="CurrentStep">The name of the step currently in progress, or the last completed step.</param>
/// <param name="CompletedSteps">The list of step results for all completed steps.</param>
/// <param name="IsComplete">Whether all onboarding steps have been completed.</param>
public sealed record OnboardingStatus(
    Guid TenantId,
    string CurrentStep,
    List<OnboardingStepResult> CompletedSteps,
    bool IsComplete);
