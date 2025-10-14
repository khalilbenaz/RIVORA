# Value Objects Guide

Value Objects are immutable types that represent domain concepts defined by their values rather than identity. The RIVORA Framework provides a `ValueObject` base class and several built-in implementations.

## ValueObject Base Class

All value objects extend `ValueObject`, which provides structural equality:

```csharp
public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    // Equality is based on all components being equal
    public override bool Equals(object? obj) { /* ... */ }
    public override int GetHashCode() { /* ... */ }
    public static bool operator ==(ValueObject? left, ValueObject? right) { /* ... */ }
    public static bool operator !=(ValueObject? left, ValueObject? right) { /* ... */ }
}
```

## Built-in Value Objects

### Email

Validates and normalizes email addresses:

```csharp
// Valid emails
var email = new Email("User@Example.COM");
Console.WriteLine(email.Value);  // "user@example.com" (normalized to lowercase)

// Invalid -- throws ArgumentException
var bad = new Email("not-an-email");

// Implicit conversion to string
string emailStr = email;

// Equality
var a = new Email("test@example.com");
var b = new Email("TEST@Example.COM");
Console.WriteLine(a == b);  // true
```

### Money

Represents a monetary amount with currency. Supports arithmetic with same-currency enforcement:

```csharp
var price = new Money(29.99m, "USD");
var tax = new Money(2.40m, "USD");

// Arithmetic
var total = price + tax;           // 32.39 USD
var discount = price * 0.10m;      // 3.00 USD
var discounted = price - discount;  // 26.99 USD

// Zero factory
var zero = Money.Zero("EUR");      // 0.00 EUR

// Comparison
Console.WriteLine(price.CompareTo(tax));  // > 0

// Different currencies throw
var eur = new Money(25.00m, "EUR");
var invalid = price + eur;  // InvalidOperationException!

// ToString
Console.WriteLine(total);  // "32.39 USD"
```

### PhoneNumber

Validates phone numbers in E.164 format:

```csharp
var phone = new PhoneNumber("+14155552671");
Console.WriteLine(phone.Value);  // "+14155552671"

// Invalid -- throws ArgumentException
var bad = new PhoneNumber("555-1234");  // Not E.164

// Implicit conversion to string
string phoneStr = phone;
```

### Address

Represents a physical/postal address:

```csharp
var address = new Address(
    street: "123 Main St",
    city: "San Francisco",
    state: "CA",
    postalCode: "94105",
    country: "US"
);

Console.WriteLine(address);
// "123 Main St, San Francisco, CA 94105, US"

// Equality
var same = new Address("123 Main St", "San Francisco", "CA", "94105", "US");
Console.WriteLine(address == same);  // true
```

### DateRange

Represents a date/time range with validation:

```csharp
var range = new DateRange(
    start: new DateTime(2026, 1, 1),
    end: new DateTime(2026, 12, 31)
);

// Duration
Console.WriteLine(range.Duration.TotalDays);  // 364

// Contains check
var july = new DateTime(2026, 7, 15);
Console.WriteLine(range.Contains(july));  // true

// Overlap detection
var q1 = new DateRange(new DateTime(2026, 1, 1), new DateTime(2026, 3, 31));
var q2 = new DateRange(new DateTime(2026, 3, 1), new DateTime(2026, 6, 30));
Console.WriteLine(q1.Overlaps(q2));  // true

// Invalid -- throws ArgumentException
var bad = new DateRange(DateTime.Now, DateTime.Now.AddDays(-1));  // Start > End!
```

## Creating Custom Value Objects

### Step 1: Extend ValueObject

```csharp
public sealed class Currency : ValueObject
{
    private static readonly HashSet<string> ValidCodes = new()
    {
        "USD", "EUR", "GBP", "JPY", "CHF", "CAD", "AUD", "MAD"
    };

    public string Code { get; }
    public string Symbol { get; }

    public Currency(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Currency code cannot be empty.", nameof(code));

        var normalized = code.Trim().ToUpperInvariant();

        if (!ValidCodes.Contains(normalized))
            throw new ArgumentException($"'{normalized}' is not a supported currency.", nameof(code));

        Code = normalized;
        Symbol = normalized switch
        {
            "USD" => "$",
            "EUR" => "\u20ac",
            "GBP" => "\u00a3",
            "JPY" => "\u00a5",
            "MAD" => "MAD",
            _ => normalized
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Code;
    }

    public override string ToString() => Code;
}
```

### Step 2: Use in entities

```csharp
public class Invoice : Entity<Guid>
{
    public Email CustomerEmail { get; private set; }
    public Money TotalAmount { get; private set; }
    public Address BillingAddress { get; private set; }
    public DateRange BillingPeriod { get; private set; }

    public Invoice(
        Email customerEmail,
        Money totalAmount,
        Address billingAddress,
        DateRange billingPeriod)
    {
        CustomerEmail = customerEmail;
        TotalAmount = totalAmount;
        BillingAddress = billingAddress;
        BillingPeriod = billingPeriod;
    }
}

// Usage
var invoice = new Invoice(
    customerEmail: new Email("john@example.com"),
    totalAmount: new Money(199.99m, "USD"),
    billingAddress: new Address("123 Main St", "New York", "NY", "10001", "US"),
    billingPeriod: new DateRange(new DateTime(2026, 3, 1), new DateTime(2026, 3, 31))
);
```

## EF Core Configuration

Configure value objects for Entity Framework Core:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Invoice>(entity =>
    {
        // Email as owned type
        entity.OwnsOne(e => e.CustomerEmail, email =>
        {
            email.Property(e => e.Value).HasColumnName("CustomerEmail").HasMaxLength(256);
        });

        // Money as owned type
        entity.OwnsOne(e => e.TotalAmount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("TotalAmount").HasPrecision(18, 2);
            money.Property(m => m.Currency).HasColumnName("TotalCurrency").HasMaxLength(3);
        });

        // Address as owned type
        entity.OwnsOne(e => e.BillingAddress, address =>
        {
            address.Property(a => a.Street).HasColumnName("BillingStreet").HasMaxLength(256);
            address.Property(a => a.City).HasColumnName("BillingCity").HasMaxLength(128);
            address.Property(a => a.State).HasColumnName("BillingState").HasMaxLength(64);
            address.Property(a => a.PostalCode).HasColumnName("BillingPostalCode").HasMaxLength(20);
            address.Property(a => a.Country).HasColumnName("BillingCountry").HasMaxLength(64);
        });

        // DateRange as owned type
        entity.OwnsOne(e => e.BillingPeriod, range =>
        {
            range.Property(r => r.Start).HasColumnName("BillingPeriodStart");
            range.Property(r => r.End).HasColumnName("BillingPeriodEnd");
        });
    });
}
```

## Best Practices

1. **Immutability**: Value objects should be immutable. All state is set in the constructor.
2. **Validation**: Validate inputs in the constructor -- an invalid value object should never exist.
3. **No identity**: Value objects are equal when their values are equal, not by ID.
4. **Self-contained**: Value objects should not reference entities or repositories.
5. **Rich behavior**: Add domain-relevant methods (e.g., `Money` arithmetic, `DateRange.Overlaps`).
