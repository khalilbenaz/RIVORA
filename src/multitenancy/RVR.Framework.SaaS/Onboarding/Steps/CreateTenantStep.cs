using Microsoft.Extensions.Logging;
using RVR.Framework.SaaS.Services;

namespace RVR.Framework.SaaS.Onboarding.Steps;

/// <summary>
/// Creates the tenant record in the backing store via <see cref="ITenantLifecycleService"/>.
/// This is typically the first step in the onboarding pipeline.
/// </summary>
public sealed class CreateTenantStep : ITenantOnboardingStep
{
    private readonly ITenantLifecycleService _lifecycleService;
    private readonly ILogger<CreateTenantStep> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="CreateTenantStep"/>.
    /// </summary>
    /// <param name="lifecycleService">The tenant lifecycle service used to provision the record.</param>
    /// <param name="logger">Logger instance.</param>
    public CreateTenantStep(
        ITenantLifecycleService lifecycleService,
        ILogger<CreateTenantStep> logger)
    {
        _lifecycleService = lifecycleService ?? throw new ArgumentNullException(nameof(lifecycleService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public string Name => "CreateTenant";

    /// <inheritdoc />
    public int Order => 100;

    /// <inheritdoc />
    public async Task<OnboardingStepResult> ExecuteAsync(
        TenantOnboardingContext context,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        _logger.LogInformation(
            "Creating tenant record for '{TenantName}' (TenantId={TenantId})",
            context.TenantName, context.TenantId);

        var request = new Models.TenantProvisionRequest
        {
            Name = context.TenantName,
            AdminEmail = context.AdminEmail,
            Plan = context.Plan,
            Subdomain = context.Properties.TryGetValue("Subdomain", out var sub)
                ? sub.ToString() ?? context.TenantName.ToLowerInvariant().Replace(" ", "-")
                : context.TenantName.ToLowerInvariant().Replace(" ", "-")
        };

        var result = await _lifecycleService.ProvisionAsync(request, ct);

        if (!result.Success)
        {
            _logger.LogWarning(
                "Failed to create tenant record for '{TenantName}': {Error}",
                context.TenantName, result.ErrorMessage);

            return new OnboardingStepResult(Name, false, result.ErrorMessage);
        }

        // Store the provisioned tenant ID so subsequent steps can reference it.
        context.Properties["ProvisionedTenantId"] = result.TenantId;

        _logger.LogInformation(
            "Tenant record created successfully (ProvisionedTenantId={ProvisionedTenantId})",
            result.TenantId);

        return new OnboardingStepResult(
            Name,
            true,
            "Tenant record created successfully",
            new Dictionary<string, object> { ["ProvisionedTenantId"] = result.TenantId });
    }

    /// <inheritdoc />
    public async Task CompensateAsync(TenantOnboardingContext context, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Properties.TryGetValue("ProvisionedTenantId", out var idObj) && idObj is Guid tenantId)
        {
            _logger.LogInformation(
                "Compensating CreateTenant: deleting tenant {TenantId} (hard delete)",
                tenantId);

            await _lifecycleService.DeleteAsync(tenantId, hardDelete: true, ct);
        }
        else
        {
            _logger.LogWarning(
                "Compensating CreateTenant: no ProvisionedTenantId found in context for tenant '{TenantName}'",
                context.TenantName);
        }
    }
}
