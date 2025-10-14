namespace RVR.Framework.Domain.ValueObjects;

/// <summary>
/// Value Object representing a monetary amount with its currency.
/// Supports basic arithmetic operations between values of the same currency.
/// </summary>
public sealed class Money : ValueObject, IComparable<Money>
{
    /// <summary>
    /// Gets the monetary amount.
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Gets the ISO 4217 currency code (e.g., "USD", "EUR").
    /// Stored in uppercase for consistent comparison.
    /// </summary>
    public string Currency { get; }

    /// <summary>
    /// Creates a new <see cref="Money"/> value object.
    /// </summary>
    /// <param name="amount">The monetary amount.</param>
    /// <param name="currency">The ISO 4217 currency code.</param>
    /// <exception cref="ArgumentException">Thrown when the currency code is null or empty.</exception>
    public Money(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency code cannot be null or empty.", nameof(currency));

        Amount = amount;
        Currency = currency.Trim().ToUpperInvariant();
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    /// <summary>
    /// Creates a zero-amount <see cref="Money"/> for the given currency.
    /// </summary>
    public static Money Zero(string currency) => new(0m, currency);

    /// <summary>
    /// Adds two monetary values. Both must share the same currency.
    /// </summary>
    public static Money operator +(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount + right.Amount, left.Currency);
    }

    /// <summary>
    /// Subtracts one monetary value from another. Both must share the same currency.
    /// </summary>
    public static Money operator -(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount - right.Amount, left.Currency);
    }

    /// <summary>
    /// Multiplies a monetary value by a scalar factor.
    /// </summary>
    public static Money operator *(Money money, decimal factor)
    {
        return new Money(money.Amount * factor, money.Currency);
    }

    /// <summary>
    /// Multiplies a scalar factor by a monetary value.
    /// </summary>
    public static Money operator *(decimal factor, Money money)
    {
        return money * factor;
    }

    /// <inheritdoc />
    public int CompareTo(Money? other)
    {
        if (other is null) return 1;
        EnsureSameCurrency(this, other);
        return Amount.CompareTo(other.Amount);
    }

    /// <inheritdoc />
    public override string ToString() => $"{Amount} {Currency}";

    private static void EnsureSameCurrency(Money left, Money right)
    {
        if (!string.Equals(left.Currency, right.Currency, StringComparison.Ordinal))
            throw new InvalidOperationException(
                $"Cannot perform arithmetic on different currencies: '{left.Currency}' and '{right.Currency}'.");
    }
}
