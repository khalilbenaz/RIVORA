namespace RVR.Framework.Domain.ValueObjects;

/// <summary>
/// Abstract base class for Value Objects in Domain-Driven Design.
/// Value Objects are immutable and compared by their structural equality
/// (i.e., two Value Objects are equal if all their components are equal).
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Returns the components that participate in equality comparison.
    /// Derived classes must override this to supply the fields/properties
    /// that define the value of the object.
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        return Equals((ValueObject)obj);
    }

    /// <inheritdoc />
    public bool Equals(ValueObject? other)
    {
        if (other is null || other.GetType() != GetType())
            return false;

        return GetEqualityComponents()
            .SequenceEqual(other.GetEqualityComponents());
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(17, (current, component) =>
                unchecked(current * 23 + (component?.GetHashCode() ?? 0)));
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }
}
