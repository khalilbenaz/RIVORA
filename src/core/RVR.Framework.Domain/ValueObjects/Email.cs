using System.Text.RegularExpressions;

namespace RVR.Framework.Domain.ValueObjects;

/// <summary>
/// Value Object representing a validated email address.
/// The value is normalized to lowercase for consistent comparison.
/// </summary>
public sealed class Email : ValueObject
{
    /// <summary>
    /// Basic email format pattern (RFC 5322 simplified).
    /// </summary>
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Gets the normalized (lowercase) email address value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a new <see cref="Email"/> value object.
    /// </summary>
    /// <param name="value">The email address string.</param>
    /// <exception cref="ArgumentException">Thrown when the email format is invalid.</exception>
    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email address cannot be null or empty.", nameof(value));

        var trimmed = value.Trim();

        if (!EmailRegex.IsMatch(trimmed))
            throw new ArgumentException($"'{trimmed}' is not a valid email address.", nameof(value));

        Value = trimmed.ToLowerInvariant();
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>
    /// Implicit conversion to string.
    /// </summary>
    public static implicit operator string(Email email) => email.Value;
}
