namespace RVR.Framework.AI.Guardrails;

/// <summary>
/// Defines the phase in which a guardrail is evaluated.
/// </summary>
public enum GuardrailPhase
{
    /// <summary>Evaluated before the request is sent to the LLM.</summary>
    BeforeRequest,

    /// <summary>Evaluated after the response is received from the LLM.</summary>
    AfterResponse,

    /// <summary>Evaluated in both phases.</summary>
    Both
}

/// <summary>
/// Indicates the severity level of a guardrail violation.
/// </summary>
public enum GuardrailSeverity
{
    /// <summary>Informational only; the request is allowed.</summary>
    Info,

    /// <summary>A warning; the request is allowed but flagged.</summary>
    Warning,

    /// <summary>The request is blocked.</summary>
    Blocked
}

/// <summary>
/// Contextual data passed to guardrails for validation.
/// </summary>
/// <param name="Input">The user input or prompt text.</param>
/// <param name="Output">The LLM output, if available (AfterResponse phase).</param>
/// <param name="ModelId">The identifier of the target model, if known.</param>
/// <param name="Metadata">Arbitrary metadata associated with the request.</param>
public sealed record GuardrailContext(
    string Input,
    string? Output,
    string? ModelId,
    Dictionary<string, object> Metadata);

/// <summary>
/// The result of a guardrail evaluation.
/// </summary>
/// <param name="IsAllowed">Whether the content passed the guardrail check.</param>
/// <param name="Reason">Human-readable reason if the content was flagged or blocked.</param>
/// <param name="Severity">The severity level of the violation.</param>
/// <param name="SanitizedContent">Optional sanitized version of the content with violations removed or masked.</param>
public sealed record GuardrailResult(
    bool IsAllowed,
    string? Reason = null,
    GuardrailSeverity Severity = GuardrailSeverity.Info,
    string? SanitizedContent = null);

/// <summary>
/// Defines a guardrail that validates LLM interactions for safety and compliance.
/// </summary>
public interface IGuardrail
{
    /// <summary>
    /// Gets the unique name of this guardrail.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the phase in which this guardrail is evaluated.
    /// </summary>
    GuardrailPhase Phase { get; }

    /// <summary>
    /// Validates the given context and returns a result indicating whether the content is allowed.
    /// </summary>
    /// <param name="context">The guardrail context containing input, output, and metadata.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A <see cref="GuardrailResult"/> indicating the validation outcome.</returns>
    Task<GuardrailResult> ValidateAsync(GuardrailContext context, CancellationToken ct = default);
}
