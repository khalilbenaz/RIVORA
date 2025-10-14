namespace RVR.Framework.Core.Helpers;

/// <summary>
/// Provides sanitization for values that will be included in log messages.
/// Prevents log forging/injection by stripping control characters that could
/// be used to inject fake log entries.
/// </summary>
public static class LogSanitizer
{
    /// <summary>
    /// Sanitizes a string value for safe inclusion in log messages by removing
    /// newline, carriage return, tab, and null characters.
    /// </summary>
    /// <param name="input">The input string to sanitize.</param>
    /// <returns>The sanitized string, or "[empty]" if the input is null or empty.</returns>
    public static string Sanitize(string? input)
    {
        if (string.IsNullOrEmpty(input)) return "[empty]";
        return input.Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace("\0", "");
    }
}
