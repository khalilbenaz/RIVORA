using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using RVR.Framework.Core.Helpers;

namespace RVR.Framework.SaaS.Onboarding;

/// <summary>
/// Default implementation of <see cref="ITenantOnboardingService"/> that orchestrates
/// tenant provisioning through an ordered pipeline of <see cref="ITenantOnboardingStep"/>
/// instances. Implements the Saga pattern: on step failure, all previously completed steps
/// are compensated in reverse order to ensure consistent state.
/// </summary>
public sealed class TenantOnboardingService : ITenantOnboardingService
{
    private readonly IEnumerable<ITenantOnboardingStep> _steps;
    private readonly ILogger<TenantOnboardingService> _logger;
    private readonly ConcurrentDictionary<Guid, TenantOnboardingContext> _contexts = new();

    /// <summary>
    /// Initializes a new instance of <see cref="TenantOnboardingService"/>.
    /// </summary>
    /// <param name="steps">All registered onboarding steps, executed in <see cref="ITenantOnboardingStep.Order"/> sequence.</param>
    /// <param name="logger">Logger instance.</param>
    public TenantOnboardingService(
        IEnumerable<ITenantOnboardingStep> steps,
        ILogger<TenantOnboardingService> logger)
    {
        _steps = steps ?? throw new ArgumentNullException(nameof(steps));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<OnboardingResult> ProvisionTenantAsync(
        TenantProvisionRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.TenantName))
            return new OnboardingResult(false, Guid.Empty, [], "Tenant name is required.");

        if (string.IsNullOrWhiteSpace(request.AdminEmail))
            return new OnboardingResult(false, Guid.Empty, [], "Admin email is required.");

        var context = new TenantOnboardingContext
        {
            TenantId = Guid.NewGuid(),
            TenantName = request.TenantName,
            Plan = request.Plan,
            AdminEmail = request.AdminEmail,
            AdminPassword = request.AdminPassword,
            Properties = request.Properties ?? new Dictionary<string, object>()
        };

        _contexts[context.TenantId] = context;

        var orderedSteps = _steps.OrderBy(s => s.Order).ToList();
        var allResults = new List<OnboardingStepResult>();

        _logger.LogInformation(
            "Starting tenant onboarding for {TenantName} (TenantId={TenantId}) with {StepCount} step(s)",
            LogSanitizer.Sanitize(context.TenantName), context.TenantId, orderedSteps.Count);

        foreach (var step in orderedSteps)
        {
            ct.ThrowIfCancellationRequested();

            _logger.LogInformation(
                "Tenant {TenantId}: executing step '{StepName}' (order={Order})",
                context.TenantId, step.Name, step.Order);

            OnboardingStepResult result;
            try
            {
                result = await step.ExecuteAsync(context, ct);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Tenant {TenantId}: step '{StepName}' threw an unhandled exception",
                    context.TenantId, step.Name);

                result = new OnboardingStepResult(step.Name, false, $"Unhandled exception: {ex.Message}");
            }

            allResults.Add(result);

            if (result.Success)
            {
                context.CompletedSteps.Add(result);
                _logger.LogInformation(
                    "Tenant {TenantId}: step '{StepName}' completed successfully",
                    context.TenantId, step.Name);
            }
            else
            {
                _logger.LogWarning(
                    "Tenant {TenantId}: step '{StepName}' failed — {Message}. Initiating compensation",
                    context.TenantId, step.Name, LogSanitizer.Sanitize(result.Message));

                await CompensateAsync(context, orderedSteps, ct);

                _contexts.TryRemove(context.TenantId, out _);

                return new OnboardingResult(
                    false,
                    context.TenantId,
                    allResults,
                    $"Onboarding failed at step '{step.Name}': {result.Message}");
            }
        }

        _logger.LogInformation(
            "Tenant {TenantId}: onboarding completed successfully with {StepCount} step(s)",
            context.TenantId, allResults.Count);

        return new OnboardingResult(true, context.TenantId, allResults);
    }

    /// <inheritdoc />
    public Task<OnboardingStatus> GetStatusAsync(Guid tenantId, CancellationToken ct = default)
    {
        if (_contexts.TryGetValue(tenantId, out var context))
        {
            var orderedSteps = _steps.OrderBy(s => s.Order).ToList();
            var completedNames = context.CompletedSteps.Select(s => s.StepName).ToHashSet();
            var currentStep = orderedSteps.FirstOrDefault(s => !completedNames.Contains(s.Name));
            var isComplete = orderedSteps.All(s => completedNames.Contains(s.Name));

            return Task.FromResult(new OnboardingStatus(
                tenantId,
                currentStep?.Name ?? (isComplete ? "Complete" : "Unknown"),
                new List<OnboardingStepResult>(context.CompletedSteps),
                isComplete));
        }

        return Task.FromResult(new OnboardingStatus(
            tenantId,
            "Unknown",
            [],
            false));
    }

    /// <summary>
    /// Compensates all completed steps in reverse order. Each compensation failure is
    /// logged but does not prevent remaining compensations from running, ensuring
    /// best-effort rollback of all committed side-effects.
    /// </summary>
    private async Task CompensateAsync(
        TenantOnboardingContext context,
        List<ITenantOnboardingStep> orderedSteps,
        CancellationToken ct)
    {
        var completedNames = context.CompletedSteps.Select(s => s.StepName).ToHashSet();
        var stepsToCompensate = orderedSteps
            .Where(s => completedNames.Contains(s.Name))
            .OrderByDescending(s => s.Order)
            .ToList();

        _logger.LogInformation(
            "Tenant {TenantId}: compensating {Count} completed step(s) in reverse order",
            context.TenantId, stepsToCompensate.Count);

        foreach (var step in stepsToCompensate)
        {
            try
            {
                _logger.LogInformation(
                    "Tenant {TenantId}: compensating step '{StepName}'",
                    context.TenantId, step.Name);

                await step.CompensateAsync(context, ct);

                _logger.LogInformation(
                    "Tenant {TenantId}: step '{StepName}' compensated successfully",
                    context.TenantId, step.Name);
            }
            catch (Exception ex)
            {
                // Log but continue compensating remaining steps to maximize rollback coverage.
                _logger.LogError(ex,
                    "Tenant {TenantId}: compensation failed for step '{StepName}'. " +
                    "Manual intervention may be required",
                    context.TenantId, step.Name);
            }
        }
    }
}
