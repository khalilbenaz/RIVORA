namespace KBA.Framework.MultiTenancy.Tests;

using Xunit;
using FluentAssertions;

/// <summary>
/// Unit tests for TenantInfo.
/// </summary>
public class TenantInfoTests
{
    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        var tenant = new TenantInfo();
        tenant.Id.Should().Be(string.Empty);
        tenant.Name.Should().Be(string.Empty);
        tenant.IsActive.Should().BeTrue();
        tenant.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var tenant = new TenantInfo()
        {
            Id = "test-id",
            Name = "Test Tenant",
            Identifier = "test",
            ConnectionString = "Connection=..."
        };
        tenant.Id.Should().Be("test-id");
        tenant.Name.Should().Be("Test Tenant");
        tenant.Identifier.Should().Be("test");
        tenant.ConnectionString.Should().Be("Connection=...");
    }

    [Fact]
    public void Metadata_CanAddValues()
    {
        var tenant = new TenantInfo();
        tenant.Metadata["Key1"] = "Value1";
        tenant.Metadata["Key2"] = "Value2";
        tenant.Metadata.Count.Should().Be(2);
        tenant.Metadata["Key1"].Should().Be("Value1");
    }
}
