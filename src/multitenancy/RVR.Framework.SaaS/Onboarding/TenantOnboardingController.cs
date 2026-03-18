using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace RVR.Framework.SaaS.Onboarding;

/// <summary>
/// API controller for tenant onboarding operations.
/// Restricted to users with the <c>SuperAdmin</c> role.
/// </summary>
[ApiController]
[Route("api/admin/tenants")]
[Authorize(Roles = "SuperAdmin")]
[Produces("application/json")]
[Tags("Tenant Onboarding")]
public sealed class TenantOnboardingController : ControllerBase
{
    private readonly ITenantOnboardingService _onboardingService;
    private readonly ILogger<TenantOnboardingController> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="TenantOnboardingController"/>.
    /// </summary>
    /// <param name="onboardingService">The tenant onboarding orchestrator.</param>
    /// <param name="logger">Logger instance.</param>
    public TenantOnboardingController(
        ITenantOnboardingService onboardingService,
        ILogger<TenantOnboardingController> logger)
    {
        _onboardingService = onboardingService ?? throw new ArgumentNullException(nameof(onboardingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Provisions a new tenant by executing the full onboarding pipeline.
    /// On failure, all completed steps are compensated (rolled back) automatically.
    /// </summary>
    /// <param name="request">The tenant provisioning request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The onboarding result including all step outcomes.</returns>
    /// <response code="200">Tenant provisioned successfully.</response>
    /// <response code="400">Invalid request or provisioning failed.</response>
    /// <response code="500">An unexpected error occurred during provisioning.</response>
    [HttpPost("provision")]
    [ProducesResponseType(typeof(OnboardingResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ProvisionTenantAsync(
        [FromBody] TenantProvisionRequest request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            _logger.LogInformation(
                "Received tenant provisioning request for '{TenantName}' on plan '{Plan}'",
                request.TenantName, request.Plan);

            var result = await _onboardingService.ProvisionTenantAsync(request, ct);

            if (!result.Success)
            {
                _logger.LogWarning(
                    "Tenant provisioning failed for '{TenantName}': {Error}",
                    request.TenantName, result.Error);

                return BadRequest(result);
            }

            _logger.LogInformation(
                "Tenant '{TenantName}' provisioned successfully (TenantId={TenantId})",
                request.TenantName, result.TenantId);

            return Ok(result);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error while provisioning tenant '{TenantName}'",
                request.TenantName);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred during tenant provisioning.");
        }
    }

    /// <summary>
    /// Gets the current onboarding status for a tenant.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The current onboarding status.</returns>
    /// <response code="200">Onboarding status retrieved successfully.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpGet("{id:guid}/status")]
    [ProducesResponseType(typeof(OnboardingStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStatusAsync(Guid id, CancellationToken ct)
    {
        try
        {
            var status = await _onboardingService.GetStatusAsync(id, ct);
            return Ok(status);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error while retrieving onboarding status for tenant {TenantId}", id);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred while retrieving onboarding status.");
        }
    }
}
