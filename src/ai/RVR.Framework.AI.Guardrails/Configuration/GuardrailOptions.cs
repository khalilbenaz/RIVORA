namespace RVR.Framework.AI.Guardrails.Configuration;

/// <summary>
/// Root configuration options for the AI Guardrails module.
/// </summary>
public sealed class GuardrailOptions
{
    /// <summary>
    /// The configuration section path for guardrail options.
    /// </summary>
    public const string SectionPath = "AI:Guardrails";

    /// <summary>
    /// Gets or sets whether guardrails are globally enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the prompt injection detection options.
    /// </summary>
    public PromptInjectionOptions PromptInjection { get; set; } = new();

    /// <summary>
    /// Gets or sets the PII detection options.
    /// </summary>
    public PiiDetectionOptions PiiDetection { get; set; } = new();

    /// <summary>
    /// Gets or sets the content moderation options.
    /// </summary>
    public ContentModerationOptions ContentModeration { get; set; } = new();

    /// <summary>
    /// Gets or sets the token budget options.
    /// </summary>
    public TokenBudgetOptions TokenBudget { get; set; } = new();

    /// <summary>
    /// Gets or sets the output validation options.
    /// </summary>
    public OutputValidationOptions OutputValidation { get; set; } = new();
}

/// <summary>
/// Configuration options for prompt injection detection.
/// </summary>
public sealed class PromptInjectionOptions
{
    /// <summary>
    /// Gets or sets whether prompt injection detection is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the severity level when an injection is detected.
    /// Valid values: Info, Warning, Blocked.
    /// </summary>
    public string Severity { get; set; } = "Blocked";
}

/// <summary>
/// Configuration options for PII detection and masking.
/// </summary>
public sealed class PiiDetectionOptions
{
    /// <summary>
    /// Gets or sets whether PII detection is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets whether detected PII should be masked in the content.
    /// When true, PII is replaced with redaction labels and the request is allowed.
    /// When false, the request is blocked.
    /// </summary>
    public bool MaskPii { get; set; } = true;
}

/// <summary>
/// Configuration options for content moderation.
/// </summary>
public sealed class ContentModerationOptions
{
    /// <summary>
    /// Gets or sets whether content moderation is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the list of terms that should be blocked.
    /// </summary>
    public List<string> BlockedTerms { get; set; } = [];
}

/// <summary>
/// Configuration options for token budget enforcement.
/// </summary>
public sealed class TokenBudgetOptions
{
    /// <summary>
    /// Gets or sets whether token budget enforcement is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of estimated tokens allowed per request.
    /// </summary>
    public int MaxTokensPerRequest { get; set; } = 4000;
}

/// <summary>
/// Configuration options for output validation.
/// </summary>
public sealed class OutputValidationOptions
{
    /// <summary>
    /// Gets or sets whether output validation is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum allowed output length in characters.
    /// </summary>
    public int MaxOutputLength { get; set; } = 10000;
}
