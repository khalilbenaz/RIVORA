namespace RVR.Framework.Core.Tests;

using Xunit;
using FluentAssertions;

/// <summary>
/// Unit tests for the Entity base class.
/// </summary>
public class EntityTests
{
    [Fact]
    public void Constructor_SetsId_ToNewGuid()
    {
        var entity = new TestEntity();
        entity.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_SetsCreatedAt_ToNow()
    {
        var before = DateTime.UtcNow;
        var entity = new TestEntity();
        var after = DateTime.UtcNow;
        entity.CreatedAt.Should().BeOnOrAfter(before);
        entity.CreatedAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void SetModified_UpdatesModifiedAtAndModifiedBy()
    {
        var entity = new TestEntity();
        var modifiedBy = "test-user";
        entity.SetModified(modifiedBy);
        entity.ModifiedAt.Should().NotBeNull();
        entity.ModifiedBy.Should().Be(modifiedBy);
    }

    [Fact]
    public void Equals_SameId_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        var entity1 = new TestEntity() { Id = id };
        var entity2 = new TestEntity() { Id = id };
        entity1.Should().Be(entity2);
    }

    [Fact]
    public void Equals_DifferentId_ReturnsFalse()
    {
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();
        entity1.Should().NotBe(entity2);
    }
}

public class TestEntity : Entity
{
    public string Name { get; set; } = string.Empty;
}
