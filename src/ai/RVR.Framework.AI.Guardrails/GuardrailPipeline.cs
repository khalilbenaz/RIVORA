using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RVR.Framework.AI.Guardrails.Configuration;

namespace RVR.Framework.AI.Guardrails;

/// <summary>
/// Pipeline that orchestrates the execution of all registered guardrails in order.
/// </summary>
public sealed class GuardrailPipeline : IGuardrailPipeline
{
    private readonly IEnumerable<IGuardrail> _guardrails;
    private readonly ILogger<GuardrailPipeline> _logger;
    private readonly GuardrailOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="GuardrailPipeline"/> class.
    /// </summary>
    /// <param name="guardrails">The registered guardrails.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The guardrail configuration options.</param>
    public GuardrailPipeline(
        IEnumerable<IGuardrail> guardrails,
        ILogger<GuardrailPipeline> logger,
        IOptions<GuardrailOptions> options)
    {
        _guardrails = guardrails ?? throw new ArgumentNullException(nameof(guardrails));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public async Task<GuardrailResult> ExecuteBeforeRequestAsync(GuardrailContext context, CancellationToken ct = default)
    {
        return await ExecuteAsync(context, GuardrailPhase.BeforeRequest, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<GuardrailResult> ExecuteAfterResponseAsync(GuardrailContext context, CancellationToken ct = default)
    {
        return await ExecuteAsync(context, GuardrailPhase.AfterResponse, ct).ConfigureAwait(false);
    }

    private async Task<GuardrailResult> ExecuteAsync(GuardrailContext context, GuardrailPhase phase, CancellationToken ct)
    {
        if (!_options.Enabled)
        {
            _logger.LogDebug("Guardrails are disabled globally; skipping {Phase} phase", phase);
            return new GuardrailResult(true);
        }

        var applicableGuardrails = _guardrails
            .Where(g => g.Phase == phase || g.Phase == GuardrailPhase.Both);

        var highestSeverity = GuardrailSeverity.Info;
        var reasons = new List<string>();
        string? sanitizedContent = null;
        var isAllowed = true;

        foreach (var guardrail in applicableGuardrails)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                _logger.LogDebug("Executing guardrail '{GuardrailName}' for {Phase} phase", guardrail.Name, phase);

                var result = await guardrail.ValidateAsync(context, ct).ConfigureAwait(false);

                if (!result.IsAllowed)
                {
                    isAllowed = false;
                    _logger.LogWarning(
                        "Guardrail '{GuardrailName}' flagged content with severity {Severity}: {Reason}",
                        guardrail.Name, result.Severity, result.Reason);

                    if (result.Reason is not null)
                    {
                        reasons.Add($"[{guardrail.Name}] {result.Reason}");
                    }

                    if (result.Severity > highestSeverity)
                    {
                        highestSeverity = result.Severity;
                    }
                }

                if (result.SanitizedContent is not null)
                {
                    sanitizedContent = result.SanitizedContent;
                    // Update context with sanitized content for subsequent guardrails
                    context = context with
                    {
                        Input = phase == GuardrailPhase.BeforeRequest ? result.SanitizedContent : context.Input,
                        Output = phase == GuardrailPhase.AfterResponse ? result.SanitizedContent : context.Output
                    };
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Guardrail '{GuardrailName}' threw an exception during {Phase} phase", guardrail.Name, phase);
                reasons.Add($"[{guardrail.Name}] Internal error during validation");
                isAllowed = false;
                highestSeverity = GuardrailSeverity.Blocked;
            }
        }

        var aggregatedReason = reasons.Count > 0 ? string.Join("; ", reasons) : null;

        if (!isAllowed)
        {
            _logger.LogWarning(
                "Guardrail pipeline {Phase} phase completed with violations: {Reason}",
                phase, aggregatedReason);
        }
        else
        {
            _logger.LogDebug("Guardrail pipeline {Phase} phase completed successfully", phase);
        }

        return new GuardrailResult(isAllowed, aggregatedReason, highestSeverity, sanitizedContent);
    }
}
