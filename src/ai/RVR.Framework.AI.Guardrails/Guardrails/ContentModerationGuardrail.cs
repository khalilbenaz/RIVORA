using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RVR.Framework.AI.Guardrails.Configuration;

namespace RVR.Framework.AI.Guardrails.Guardrails;

/// <summary>
/// Checks for forbidden content patterns based on a configurable blocklist of terms.
/// </summary>
public sealed class ContentModerationGuardrail : IGuardrail
{
    private readonly ILogger<ContentModerationGuardrail> _logger;
    private readonly ContentModerationOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentModerationGuardrail"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The content moderation options.</param>
    public ContentModerationGuardrail(
        ILogger<ContentModerationGuardrail> logger,
        IOptions<ContentModerationOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public string Name => "ContentModeration";

    /// <inheritdoc />
    public GuardrailPhase Phase => GuardrailPhase.Both;

    /// <inheritdoc />
    public Task<GuardrailResult> ValidateAsync(GuardrailContext context, CancellationToken ct = default)
    {
        if (!_options.Enabled)
        {
            return Task.FromResult(new GuardrailResult(true));
        }

        if (_options.BlockedTerms is null || _options.BlockedTerms.Count == 0)
        {
            return Task.FromResult(new GuardrailResult(true));
        }

        var textToScan = context.Output ?? context.Input;
        if (string.IsNullOrWhiteSpace(textToScan))
        {
            return Task.FromResult(new GuardrailResult(true));
        }

        var normalizedText = textToScan.ToUpperInvariant();
        var matchedTerms = new List<string>();

        foreach (var term in _options.BlockedTerms)
        {
            ct.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(term))
            {
                continue;
            }

            if (normalizedText.Contains(term.ToUpperInvariant(), StringComparison.Ordinal))
            {
                matchedTerms.Add(term);
                _logger.LogWarning("Blocked term detected in content: '{Term}'", term);
            }
        }

        if (matchedTerms.Count == 0)
        {
            return Task.FromResult(new GuardrailResult(true));
        }

        return Task.FromResult(new GuardrailResult(
            false,
            $"Content contains blocked terms: {string.Join(", ", matchedTerms)}",
            GuardrailSeverity.Blocked));
    }
}
