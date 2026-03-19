using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RVR.Framework.AI.Guardrails.Configuration;

namespace RVR.Framework.AI.Guardrails.Guardrails;

/// <summary>
/// Validates that LLM output matches expected format constraints including
/// JSON validity and maximum length. Runs in the AfterResponse phase only.
/// </summary>
public sealed class OutputSchemaGuardrail : IGuardrail
{
    private readonly ILogger<OutputSchemaGuardrail> _logger;
    private readonly OutputValidationOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="OutputSchemaGuardrail"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The output validation options.</param>
    public OutputSchemaGuardrail(
        ILogger<OutputSchemaGuardrail> logger,
        IOptions<OutputValidationOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public string Name => "OutputSchema";

    /// <inheritdoc />
    public GuardrailPhase Phase => GuardrailPhase.AfterResponse;

    /// <inheritdoc />
    public Task<GuardrailResult> ValidateAsync(GuardrailContext context, CancellationToken ct = default)
    {
        if (!_options.Enabled)
        {
            return Task.FromResult(new GuardrailResult(true));
        }

        var output = context.Output;
        if (string.IsNullOrWhiteSpace(output))
        {
            return Task.FromResult(new GuardrailResult(true));
        }

        // Check maximum length
        if (output.Length > _options.MaxOutputLength)
        {
            _logger.LogWarning(
                "Output length {OutputLength} exceeds maximum allowed length {MaxLength}",
                output.Length, _options.MaxOutputLength);

            return Task.FromResult(new GuardrailResult(
                false,
                $"Output length ({output.Length}) exceeds maximum allowed ({_options.MaxOutputLength})",
                GuardrailSeverity.Blocked));
        }

        // Check JSON validity if the output appears to be JSON
        var trimmed = output.AsSpan().Trim();
        if (trimmed.Length > 0 && (trimmed[0] == '{' || trimmed[0] == '['))
        {
            if (!IsValidJson(output))
            {
                _logger.LogWarning("Output appears to be JSON but is not valid JSON");

                return Task.FromResult(new GuardrailResult(
                    false,
                    "Output appears to be JSON but is not valid",
                    GuardrailSeverity.Warning));
            }
        }

        return Task.FromResult(new GuardrailResult(true));
    }

    private static bool IsValidJson(string text)
    {
        try
        {
            using var doc = JsonDocument.Parse(text);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
