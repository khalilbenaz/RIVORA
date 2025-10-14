namespace RVR.Framework.Domain.Common;

/// <summary>
/// Structured error with code, message, details, and inner errors for rich error reporting.
/// </summary>
public class Error
{
    /// <summary>
    /// Machine-readable error code (e.g., "VALIDATION_FAILED", "NOT_FOUND").
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Human-readable error message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Additional key-value details about the error.
    /// </summary>
    public IReadOnlyDictionary<string, object> Details { get; }

    /// <summary>
    /// Nested errors for composite failure scenarios.
    /// </summary>
    public IReadOnlyList<Error> InnerErrors { get; }

    public Error(
        string code,
        string message,
        IDictionary<string, object>? details = null,
        IEnumerable<Error>? innerErrors = null)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Details = details != null
            ? new Dictionary<string, object>(details).AsReadOnly()
            : new Dictionary<string, object>().AsReadOnly();
        InnerErrors = innerErrors != null
            ? innerErrors.ToList().AsReadOnly()
            : new List<Error>().AsReadOnly();
    }

    public override string ToString() => $"[{Code}] {Message}";
}
