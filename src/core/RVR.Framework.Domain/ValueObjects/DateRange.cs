namespace RVR.Framework.Domain.ValueObjects;

/// <summary>
/// Value Object representing a date/time range with a start and end.
/// Enforces that <see cref="Start"/> is less than or equal to <see cref="End"/>.
/// </summary>
public sealed class DateRange : ValueObject
{
    /// <summary>
    /// Gets the start of the date range (inclusive).
    /// </summary>
    public DateTime Start { get; }

    /// <summary>
    /// Gets the end of the date range (inclusive).
    /// </summary>
    public DateTime End { get; }

    /// <summary>
    /// Creates a new <see cref="DateRange"/> value object.
    /// </summary>
    /// <param name="start">The start date/time (inclusive).</param>
    /// <param name="end">The end date/time (inclusive).</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="start"/> is after <paramref name="end"/>.</exception>
    public DateRange(DateTime start, DateTime end)
    {
        if (start > end)
            throw new ArgumentException(
                $"Start date ({start:O}) must be less than or equal to end date ({end:O}).",
                nameof(start));

        Start = start;
        End = end;
    }

    /// <summary>
    /// Gets the duration of this date range.
    /// </summary>
    public TimeSpan Duration => End - Start;

    /// <summary>
    /// Determines whether the specified <paramref name="dateTime"/> falls within this range (inclusive).
    /// </summary>
    /// <param name="dateTime">The date/time to check.</param>
    /// <returns><c>true</c> if the value is within the range; otherwise <c>false</c>.</returns>
    public bool Contains(DateTime dateTime)
    {
        return dateTime >= Start && dateTime <= End;
    }

    /// <summary>
    /// Determines whether this range overlaps with another <see cref="DateRange"/>.
    /// Two ranges overlap when one starts before the other ends and vice versa.
    /// </summary>
    /// <param name="other">The other date range.</param>
    /// <returns><c>true</c> if the ranges overlap; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="other"/> is null.</exception>
    public bool Overlaps(DateRange other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return Start <= other.End && other.Start <= End;
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Start;
        yield return End;
    }

    /// <inheritdoc />
    public override string ToString() => $"{Start:O} - {End:O}";
}
