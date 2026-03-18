using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using RVR.Framework.Core.Helpers;
using RVR.Framework.SaaS.Models;

namespace RVR.Framework.SaaS.Services;

/// <summary>
/// Default implementation of <see cref="ITenantLifecycleService"/> that manages tenant
/// provisioning, suspension, reactivation, and deletion using an in-memory store.
/// Replace the backing store with a persistent repository for production use.
/// </summary>
public sealed class TenantLifecycleService : ITenantLifecycleService
{
    private readonly ConcurrentDictionary<Guid, TenantRecord> _tenants = new();
    private readonly ILogger<TenantLifecycleService> _logger;

    public TenantLifecycleService(ILogger<TenantLifecycleService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TenantProvisionResult> ProvisionAsync(
        TenantProvisionRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Name))
            return TenantProvisionResult.Failed("Tenant name is required.");

        if (string.IsNullOrWhiteSpace(request.AdminEmail))
            return TenantProvisionResult.Failed("Admin email is required.");

        if (string.IsNullOrWhiteSpace(request.Subdomain))
            return TenantProvisionResult.Failed("Subdomain is required.");

        // Check for duplicate subdomain
        foreach (var kvp in _tenants)
        {
            if (string.Equals(kvp.Value.Subdomain, request.Subdomain, StringComparison.OrdinalIgnoreCase))
                return TenantProvisionResult.Failed($"Subdomain '{request.Subdomain}' is already in use.");
        }

        var tenantId = Guid.NewGuid();
        var record = new TenantRecord
        {
            TenantId = tenantId,
            Name = request.Name,
            AdminEmail = request.AdminEmail,
            Plan = request.Plan,
            Subdomain = request.Subdomain,
            Status = TenantStatus.Provisioning,
            CreatedAt = DateTime.UtcNow
        };

        if (!_tenants.TryAdd(tenantId, record))
            return TenantProvisionResult.Failed("Failed to create tenant record.");

        _logger.LogInformation("Provisioning tenant {TenantId} ({Name})", tenantId, LogSanitizer.Sanitize(request.Name));

        // Step 1: Create database
        await CreateDatabaseAsync(record, ct);
        record.Onboarding.DatabaseCreated = true;
        record.Onboarding.CompletedSteps.Add("DatabaseCreated");
        _logger.LogInformation("Tenant {TenantId}: database created", tenantId);

        // Step 2: Create admin user
        await CreateAdminUserAsync(record, ct);
        record.Onboarding.AdminUserCreated = true;
        record.Onboarding.CompletedSteps.Add("AdminUserCreated");
        _logger.LogInformation("Tenant {TenantId}: admin user created for {Email}", tenantId, LogSanitizer.Sanitize(request.AdminEmail));

        // Step 3: Configure default settings
        await ConfigureDefaultSettingsAsync(record, ct);
        record.Onboarding.SettingsConfigured = true;
        record.Onboarding.CompletedSteps.Add("SettingsConfigured");
        _logger.LogInformation("Tenant {TenantId}: default settings configured", tenantId);

        // Step 4: Send welcome email
        await SendWelcomeEmailAsync(record, ct);
        record.Onboarding.WelcomeEmailSent = true;
        record.Onboarding.CompletedSteps.Add("WelcomeEmailSent");
        _logger.LogInformation("Tenant {TenantId}: welcome email sent", tenantId);

        // Mark as active once all steps are complete
        record.Status = TenantStatus.Active;
        record.Onboarding.Status = TenantStatus.Active;

        _logger.LogInformation("Tenant {TenantId} provisioning complete", tenantId);

        return TenantProvisionResult.Succeeded(tenantId);
    }

    /// <inheritdoc />
    public Task<bool> SuspendAsync(Guid tenantId, string reason, CancellationToken ct = default)
    {
        if (!_tenants.TryGetValue(tenantId, out var record))
        {
            _logger.LogWarning("Cannot suspend tenant {TenantId}: not found", tenantId);
            return Task.FromResult(false);
        }

        if (record.Status is TenantStatus.Suspended or TenantStatus.Deactivated)
        {
            _logger.LogWarning("Cannot suspend tenant {TenantId}: current status is {Status}", tenantId, record.Status);
            return Task.FromResult(false);
        }

        record.Status = TenantStatus.Suspended;
        record.SuspensionReason = reason;
        record.SuspendedAt = DateTime.UtcNow;

        _logger.LogInformation("Tenant {TenantId} suspended. Reason: {Reason}", tenantId, reason);
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<bool> ReactivateAsync(Guid tenantId, CancellationToken ct = default)
    {
        if (!_tenants.TryGetValue(tenantId, out var record))
        {
            _logger.LogWarning("Cannot reactivate tenant {TenantId}: not found", tenantId);
            return Task.FromResult(false);
        }

        if (record.Status != TenantStatus.Suspended)
        {
            _logger.LogWarning("Cannot reactivate tenant {TenantId}: not currently suspended (status: {Status})", tenantId, record.Status);
            return Task.FromResult(false);
        }

        record.Status = TenantStatus.Active;
        record.SuspensionReason = null;
        record.SuspendedAt = null;

        _logger.LogInformation("Tenant {TenantId} reactivated", tenantId);
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(Guid tenantId, bool hardDelete = false, CancellationToken ct = default)
    {
        if (!_tenants.TryGetValue(tenantId, out var record))
        {
            _logger.LogWarning("Cannot delete tenant {TenantId}: not found", tenantId);
            return Task.FromResult(false);
        }

        if (hardDelete)
        {
            // Hard delete: remove all data
            _tenants.TryRemove(tenantId, out _);
            _logger.LogInformation("Tenant {TenantId} hard-deleted", tenantId);
        }
        else
        {
            // Soft delete: mark as deactivated for future cleanup
            record.Status = TenantStatus.Deactivated;
            record.DeactivatedAt = DateTime.UtcNow;
            _logger.LogInformation("Tenant {TenantId} soft-deleted (deactivated)", tenantId);
        }

        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<TenantOnboardingStatus> GetOnboardingStatusAsync(Guid tenantId, CancellationToken ct = default)
    {
        if (!_tenants.TryGetValue(tenantId, out var record))
        {
            return Task.FromResult(new TenantOnboardingStatus
            {
                TenantId = tenantId,
                Status = TenantStatus.Deactivated
            });
        }

        var status = new TenantOnboardingStatus
        {
            TenantId = tenantId,
            Status = record.Status,
            DatabaseCreated = record.Onboarding.DatabaseCreated,
            AdminUserCreated = record.Onboarding.AdminUserCreated,
            SettingsConfigured = record.Onboarding.SettingsConfigured,
            WelcomeEmailSent = record.Onboarding.WelcomeEmailSent,
            CompletedSteps = new List<string>(record.Onboarding.CompletedSteps)
        };

        return Task.FromResult(status);
    }

    // --- Simulated provisioning steps (replace with real implementations) ---

    private Task CreateDatabaseAsync(TenantRecord record, CancellationToken ct)
    {
        // In production: create a database or schema for the tenant
        return Task.CompletedTask;
    }

    private Task CreateAdminUserAsync(TenantRecord record, CancellationToken ct)
    {
        // In production: create the initial admin user with the provided email
        return Task.CompletedTask;
    }

    private Task ConfigureDefaultSettingsAsync(TenantRecord record, CancellationToken ct)
    {
        // In production: seed default configuration, roles, permissions, etc.
        return Task.CompletedTask;
    }

    private Task SendWelcomeEmailAsync(TenantRecord record, CancellationToken ct)
    {
        // In production: send onboarding/welcome email to admin
        return Task.CompletedTask;
    }

    /// <summary>
    /// Internal record used to track tenant state in the in-memory store.
    /// </summary>
    private sealed class TenantRecord
    {
        public Guid TenantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;
        public string Subdomain { get; set; } = string.Empty;
        public TenantStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? SuspensionReason { get; set; }
        public DateTime? SuspendedAt { get; set; }
        public DateTime? DeactivatedAt { get; set; }
        public OnboardingState Onboarding { get; } = new();
    }

    private sealed class OnboardingState
    {
        public bool DatabaseCreated { get; set; }
        public bool AdminUserCreated { get; set; }
        public bool SettingsConfigured { get; set; }
        public bool WelcomeEmailSent { get; set; }
        public TenantStatus Status { get; set; }
        public List<string> CompletedSteps { get; } = [];
    }
}
