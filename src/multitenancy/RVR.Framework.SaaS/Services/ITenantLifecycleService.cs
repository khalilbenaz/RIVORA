using RVR.Framework.SaaS.Models;

namespace RVR.Framework.SaaS.Services;

/// <summary>
/// Manages the full lifecycle of a SaaS tenant including provisioning,
/// suspension, reactivation, and deletion.
/// </summary>
public interface ITenantLifecycleService
{
    /// <summary>
    /// Provisions a new tenant with database, admin user, default settings, and welcome email.
    /// </summary>
    Task<TenantProvisionResult> ProvisionAsync(TenantProvisionRequest request, CancellationToken ct = default);

    /// <summary>
    /// Suspends a tenant, blocking all API access.
    /// </summary>
    Task<bool> SuspendAsync(Guid tenantId, string reason, CancellationToken ct = default);

    /// <summary>
    /// Reactivates a previously suspended tenant.
    /// </summary>
    Task<bool> ReactivateAsync(Guid tenantId, CancellationToken ct = default);

    /// <summary>
    /// Deletes a tenant. Soft delete marks for future cleanup; hard delete removes all data.
    /// </summary>
    Task<bool> DeleteAsync(Guid tenantId, bool hardDelete = false, CancellationToken ct = default);

    /// <summary>
    /// Returns the current onboarding status for a tenant, including which steps are completed.
    /// </summary>
    Task<TenantOnboardingStatus> GetOnboardingStatusAsync(Guid tenantId, CancellationToken ct = default);
}
