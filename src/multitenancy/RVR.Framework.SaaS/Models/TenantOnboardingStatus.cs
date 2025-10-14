namespace RVR.Framework.SaaS.Models;

/// <summary>
/// Tracks the completion status of onboarding steps for a tenant.
/// </summary>
public sealed class TenantOnboardingStatus
{
    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the current status of the tenant.
    /// </summary>
    public TenantStatus Status { get; set; }

    /// <summary>
    /// Gets or sets whether the tenant database has been created.
    /// </summary>
    public bool DatabaseCreated { get; set; }

    /// <summary>
    /// Gets or sets whether the admin user has been created.
    /// </summary>
    public bool AdminUserCreated { get; set; }

    /// <summary>
    /// Gets or sets whether default settings have been configured.
    /// </summary>
    public bool SettingsConfigured { get; set; }

    /// <summary>
    /// Gets or sets whether the welcome email has been sent.
    /// </summary>
    public bool WelcomeEmailSent { get; set; }

    /// <summary>
    /// Gets the list of completed onboarding steps.
    /// </summary>
    public List<string> CompletedSteps { get; set; } = [];

    /// <summary>
    /// Gets whether all onboarding steps have been completed.
    /// </summary>
    public bool IsComplete =>
        DatabaseCreated && AdminUserCreated && SettingsConfigured && WelcomeEmailSent;
}
