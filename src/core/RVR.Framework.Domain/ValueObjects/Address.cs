namespace RVR.Framework.Domain.ValueObjects;

/// <summary>
/// Value Object representing a physical/postal address.
/// </summary>
public sealed class Address : ValueObject
{
    /// <summary>
    /// Gets the street line (e.g., "123 Main St").
    /// </summary>
    public string Street { get; }

    /// <summary>
    /// Gets the city name.
    /// </summary>
    public string City { get; }

    /// <summary>
    /// Gets the state or province.
    /// </summary>
    public string State { get; }

    /// <summary>
    /// Gets the postal / ZIP code.
    /// </summary>
    public string PostalCode { get; }

    /// <summary>
    /// Gets the country name or ISO code.
    /// </summary>
    public string Country { get; }

    /// <summary>
    /// Creates a new <see cref="Address"/> value object.
    /// </summary>
    /// <param name="street">Street line.</param>
    /// <param name="city">City name.</param>
    /// <param name="state">State or province.</param>
    /// <param name="postalCode">Postal / ZIP code.</param>
    /// <param name="country">Country name or code.</param>
    /// <exception cref="ArgumentException">Thrown when any required component is null or empty.</exception>
    public Address(string street, string city, string state, string postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be null or empty.", nameof(street));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be null or empty.", nameof(city));
        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State cannot be null or empty.", nameof(state));
        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("Postal code cannot be null or empty.", nameof(postalCode));
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be null or empty.", nameof(country));

        Street = street.Trim();
        City = city.Trim();
        State = state.Trim();
        PostalCode = postalCode.Trim();
        Country = country.Trim();
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return PostalCode;
        yield return Country;
    }

    /// <inheritdoc />
    public override string ToString() => $"{Street}, {City}, {State} {PostalCode}, {Country}";
}
