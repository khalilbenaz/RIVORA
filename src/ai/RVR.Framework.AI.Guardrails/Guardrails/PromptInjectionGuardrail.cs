using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RVR.Framework.AI.Guardrails.Configuration;

namespace RVR.Framework.AI.Guardrails.Guardrails;

/// <summary>
/// Detects prompt injection patterns such as system prompt override attempts,
/// jailbreak patterns, and role manipulation.
/// </summary>
public sealed partial class PromptInjectionGuardrail : IGuardrail
{
    private readonly ILogger<PromptInjectionGuardrail> _logger;
    private readonly PromptInjectionOptions _options;

    private static readonly (Regex Pattern, string Description)[] InjectionPatterns =
    [
        (IgnorePreviousRegex(), "Ignore previous instructions pattern"),
        (YouAreNowRegex(), "Role reassignment pattern"),
        (SystemPromptRegex(), "System prompt override pattern"),
        (DisregardRegex(), "Disregard instructions pattern"),
        (ActAsRegex(), "Act-as role manipulation pattern"),
        (NewInstructionsRegex(), "New instructions injection pattern"),
        (ForgetRegex(), "Forget instructions pattern"),
        (OverrideRegex(), "Override instructions pattern"),
        (JailbreakRegex(), "Jailbreak attempt pattern"),
        (PretendRegex(), "Pretend role manipulation pattern"),
    ];

    /// <summary>
    /// Initializes a new instance of the <see cref="PromptInjectionGuardrail"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The prompt injection options.</param>
    public PromptInjectionGuardrail(
        ILogger<PromptInjectionGuardrail> logger,
        IOptions<PromptInjectionOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public string Name => "PromptInjection";

    /// <inheritdoc />
    public GuardrailPhase Phase => GuardrailPhase.BeforeRequest;

    /// <inheritdoc />
    public Task<GuardrailResult> ValidateAsync(GuardrailContext context, CancellationToken ct = default)
    {
        if (!_options.Enabled)
        {
            return Task.FromResult(new GuardrailResult(true));
        }

        var input = context.Input;
        if (string.IsNullOrWhiteSpace(input))
        {
            return Task.FromResult(new GuardrailResult(true));
        }

        foreach (var (pattern, description) in InjectionPatterns)
        {
            ct.ThrowIfCancellationRequested();

            if (pattern.IsMatch(input))
            {
                var severity = Enum.TryParse<GuardrailSeverity>(_options.Severity, true, out var parsed)
                    ? parsed
                    : GuardrailSeverity.Blocked;

                _logger.LogWarning(
                    "Prompt injection detected: {Description} in input",
                    description);

                return Task.FromResult(new GuardrailResult(
                    false,
                    $"Prompt injection detected: {description}",
                    severity));
            }
        }

        return Task.FromResult(new GuardrailResult(true));
    }

    [GeneratedRegex(@"ignore\s+(all\s+)?previous\s+(instructions|prompts|rules)", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex IgnorePreviousRegex();

    [GeneratedRegex(@"you\s+are\s+now\s+(a|an|the)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex YouAreNowRegex();

    [GeneratedRegex(@"\[?\s*system\s*(prompt|message|instruction)\s*\]?.*:", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex SystemPromptRegex();

    [GeneratedRegex(@"disregard\s+(all\s+)?(previous|prior|above|earlier)\s+(instructions|prompts|rules)", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex DisregardRegex();

    [GeneratedRegex(@"(act|behave|respond)\s+as\s+(if\s+you\s+are|though\s+you\s+are|a|an)", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ActAsRegex();

    [GeneratedRegex(@"(new|updated|revised)\s+(instructions|rules|prompt)\s*:", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex NewInstructionsRegex();

    [GeneratedRegex(@"forget\s+(all\s+)?(previous|prior|your)\s+(instructions|training|rules|context)", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ForgetRegex();

    [GeneratedRegex(@"override\s+(your\s+)?(instructions|rules|guidelines|safety)", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex OverrideRegex();

    [GeneratedRegex(@"\b(jailbreak|DAN|do\s+anything\s+now)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex JailbreakRegex();

    [GeneratedRegex(@"pretend\s+(you\s+are|to\s+be|you'?re)\s+(a|an|the|not)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex PretendRegex();
}
