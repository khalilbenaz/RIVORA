using System.Text.RegularExpressions;

namespace RVR.Framework.Domain.ValueObjects;

/// <summary>
/// Value Object representing a phone number in E.164 format.
/// E.164 numbers start with '+' followed by 1-15 digits (e.g., +14155552671).
/// </summary>
public sealed class PhoneNumber : ValueObject
{
    /// <summary>
    /// E.164 format: '+' followed by 1 to 15 digits.
    /// </summary>
    private static readonly Regex E164Regex = new(
        @"^\+[1-9]\d{1,14}$",
        RegexOptions.Compiled);

    /// <summary>
    /// Gets the phone number value in E.164 format.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a new <see cref="PhoneNumber"/> value object.
    /// </summary>
    /// <param name="value">The phone number string in E.164 format.</param>
    /// <exception cref="ArgumentException">Thrown when the phone number is not in E.164 format.</exception>
    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone number cannot be null or empty.", nameof(value));

        var trimmed = value.Trim();

        if (!E164Regex.IsMatch(trimmed))
            throw new ArgumentException(
                $"'{trimmed}' is not a valid E.164 phone number. Expected format: +[country code][number], e.g., +14155552671.",
                nameof(value));

        Value = trimmed;
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
    public static implicit operator string(PhoneNumber phone) => phone.Value;
}
