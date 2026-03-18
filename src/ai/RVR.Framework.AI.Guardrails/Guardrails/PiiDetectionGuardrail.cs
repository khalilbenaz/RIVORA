using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RVR.Framework.AI.Guardrails.Configuration;

namespace RVR.Framework.AI.Guardrails.Guardrails;

/// <summary>
/// Detects and optionally masks Personally Identifiable Information (PII)
/// including emails, phone numbers, credit card numbers, SSN, and French NIR (securite sociale).
/// </summary>
public sealed partial class PiiDetectionGuardrail : IGuardrail
{
    private readonly ILogger<PiiDetectionGuardrail> _logger;
    private readonly PiiDetectionOptions _options;

    private static readonly (Regex Pattern, string RedactionLabel)[] PiiPatterns =
    [
        (EmailRegex(), "[EMAIL_REDACTED]"),
        (CreditCardRegex(), "[CREDIT_CARD_REDACTED]"),
        (SsnRegex(), "[SSN_REDACTED]"),
        (FrenchNirRegex(), "[NIR_REDACTED]"),
        (PhoneRegex(), "[PHONE_REDACTED]"),
    ];

    /// <summary>
    /// Initializes a new instance of the <see cref="PiiDetectionGuardrail"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The PII detection options.</param>
    public PiiDetectionGuardrail(
        ILogger<PiiDetectionGuardrail> logger,
        IOptions<PiiDetectionOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public string Name => "PiiDetection";

    /// <inheritdoc />
    public GuardrailPhase Phase => GuardrailPhase.Both;

    /// <inheritdoc />
    public Task<GuardrailResult> ValidateAsync(GuardrailContext context, CancellationToken ct = default)
    {
        if (!_options.Enabled)
        {
            return Task.FromResult(new GuardrailResult(true));
        }

        var textToScan = context.Output ?? context.Input;
        if (string.IsNullOrWhiteSpace(textToScan))
        {
            return Task.FromResult(new GuardrailResult(true));
        }

        var detectedTypes = new List<string>();
        var sanitized = textToScan;

        foreach (var (pattern, label) in PiiPatterns)
        {
            ct.ThrowIfCancellationRequested();

            if (pattern.IsMatch(sanitized))
            {
                var typeName = label.TrimStart('[').TrimEnd(']');
                detectedTypes.Add(typeName);

                _logger.LogWarning("PII detected: {PiiType} found in content", typeName);

                if (_options.MaskPii)
                {
                    sanitized = pattern.Replace(sanitized, label);
                }
            }
        }

        if (detectedTypes.Count == 0)
        {
            return Task.FromResult(new GuardrailResult(true));
        }

        var reason = $"PII detected: {string.Join(", ", detectedTypes)}";

        return Task.FromResult(new GuardrailResult(
            IsAllowed: _options.MaskPii,
            Reason: reason,
            Severity: _options.MaskPii ? GuardrailSeverity.Warning : GuardrailSeverity.Blocked,
            SanitizedContent: _options.MaskPii ? sanitized : null));
    }

    // Email: standard email pattern
    [GeneratedRegex(@"[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex EmailRegex();

    // Credit card: 13-19 digit sequences with optional separators
    [GeneratedRegex(@"\b(?:\d[ \-]*?){13,19}\b", RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex CreditCardRegex();

    // US SSN: ###-##-#### format
    [GeneratedRegex(@"\b\d{3}-\d{2}-\d{4}\b", RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex SsnRegex();

    // French NIR (securite sociale): 1 or 2 followed by 12 digits with optional key (2 digits)
    [GeneratedRegex(@"\b[12]\s?\d{2}\s?\d{2}\s?\d{2}\s?\d{3}\s?\d{3}\s?(?:\d{2})?\b", RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex FrenchNirRegex();

    // Phone numbers: international and local formats
    [GeneratedRegex(@"(?:\+\d{1,3}[-.\s]?)?\(?\d{2,4}\)?[-.\s]?\d{2,4}[-.\s]?\d{2,4}(?:[-.\s]?\d{2,4})?", RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex PhoneRegex();
}
