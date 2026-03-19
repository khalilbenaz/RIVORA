namespace RVR.Framework.AI.Guardrails;

/// <summary>
/// Defines the pipeline that orchestrates guardrail execution.
/// </summary>
public interface IGuardrailPipeline
{
    /// <summary>
    /// Executes all registered guardrails for the <see cref="GuardrailPhase.BeforeRequest"/> phase.
    /// </summary>
    /// <param name="context">The guardrail context.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>An aggregated <see cref="GuardrailResult"/>.</returns>
    Task<GuardrailResult> ExecuteBeforeRequestAsync(GuardrailContext context, CancellationToken ct = default);

    /// <summary>
    /// Executes all registered guardrails for the <see cref="GuardrailPhase.AfterResponse"/> phase.
    /// </summary>
    /// <param name="context">The guardrail context.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>An aggregated <see cref="GuardrailResult"/>.</returns>
    Task<GuardrailResult> ExecuteAfterResponseAsync(GuardrailContext context, CancellationToken ct = default);
}
