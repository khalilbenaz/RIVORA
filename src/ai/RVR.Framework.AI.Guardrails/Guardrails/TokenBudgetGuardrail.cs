using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RVR.Framework.AI.Guardrails.Configuration;

namespace RVR.Framework.AI.Guardrails.Guardrails;

/// <summary>
/// Limits the approximate token count per request based on a configurable maximum.
/// Token estimation uses a word count multiplied by 1.3 as an approximation.
/// </summary>
public sealed class TokenBudgetGuardrail : IGuardrail
{
    private const double TokensPerWordEstimate = 1.3;

    private readonly ILogger<TokenBudgetGuardrail> _logger;
    private readonly TokenBudgetOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenBudgetGuardrail"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The token budget options.</param>
    public TokenBudgetGuardrail(
        ILogger<TokenBudgetGuardrail> logger,
        IOptions<TokenBudgetOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public string Name => "TokenBudget";

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

        var wordCount = input.Split(
            (char[]?) null,
            StringSplitOptions.RemoveEmptyEntries).Length;

        var estimatedTokens = (int)Math.Ceiling(wordCount * TokensPerWordEstimate);

        if (estimatedTokens <= _options.MaxTokensPerRequest)
        {
            _logger.LogDebug(
                "Token budget check passed: estimated {EstimatedTokens} tokens (max {MaxTokens})",
                estimatedTokens, _options.MaxTokensPerRequest);

            return Task.FromResult(new GuardrailResult(true));
        }

        _logger.LogWarning(
            "Token budget exceeded: estimated {EstimatedTokens} tokens exceeds maximum of {MaxTokens}",
            estimatedTokens, _options.MaxTokensPerRequest);

        return Task.FromResult(new GuardrailResult(
            false,
            $"Estimated token count ({estimatedTokens}) exceeds the maximum allowed ({_options.MaxTokensPerRequest})",
            GuardrailSeverity.Blocked));
    }
}
