namespace RVR.Framework.Core.Tests;

using Xunit;
using FluentAssertions;

/// <summary>
/// Unit tests for the AggregateRoot base class.
/// </summary>
public class AggregateRootTests
{
    [Fact]
    public void Constructor_SetsVersionToOne()
    {
        var aggregate = new TestAggregate();
        aggregate.Version.Should().Be(1);
    }

    [Fact]
    public void MarkAsDeleted_SetsIsDeletedToTrue()
    {
        var aggregate = new TestAggregate();
        aggregate.MarkAsDeleted();
        aggregate.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void IncrementVersion_IncrementsVersionByOne()
    {
        var aggregate = new TestAggregate();
        aggregate.IncrementVersion();
        aggregate.Version.Should().Be(2);
    }

    [Fact]
    public void Apply_RaisesDomainEvent()
    {
        var aggregate = new TestAggregate();
        aggregate.TriggerEvent();
        aggregate.DomainEvents.Count.Should().Be(1);
    }
}

public class TestAggregate : AggregateRoot
{
    public string Data { get; set; } = string.Empty;
    public void TriggerEvent() => Apply(new TestDomainEvent());
}

public class TestDomainEvent : IDomainEvent
{
    public DateTime OccurredOn => DateTime.UtcNow;
}
