namespace KBA.Framework.Features.Tests.Core;

using KBA.Framework.Features.Core;
using FluentAssertions;
using Xunit;

/// <summary>
/// Unit tests for the <see cref="FeatureInfo"/> class.
/// </summary>
public class FeatureInfoTests
{
    [Fact]
    public void Constructor_InitializesWithDefaultValues()
    {
        // Arrange & Act
        var feature = new FeatureInfo();

        // Assert
        feature.Name.Should().BeEmpty();
        feature.Enabled.Should().BeFalse();
        feature.Description.Should().BeEmpty();
        feature.Metadata.Should().NotBeNull();
        feature.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithName_SetsName()
    {
        // Arrange
        var featureName = "TestFeature";

        // Act
        var feature = new FeatureInfo(featureName);

        // Assert
        feature.Name.Should().Be(featureName);
    }

    [Fact]
    public void Constructor_WithNullName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FeatureInfo(null!));
    }

    [Fact]
    public void SetMetadata_AddsNewKey()
    {
        // Arrange
        var feature = new FeatureInfo("TestFeature");

        // Act
        feature.SetMetadata("key1", "value1");

        // Assert
        feature.Metadata.Should().ContainKey("key1").WhoseValue.Should().Be("value1");
    }

    [Fact]
    public void SetMetadata_UpdatesExistingKey()
    {
        // Arrange
        var feature = new FeatureInfo("TestFeature");
        feature.SetMetadata("key1", "value1");

        // Act
        feature.SetMetadata("key1", "value2");

        // Assert
        feature.Metadata.Should().ContainKey("key1").WhoseValue.Should().Be("value2");
    }

    [Fact]
    public void SetMetadata_WithNullKey_ThrowsArgumentNullException()
    {
        // Arrange
        var feature = new FeatureInfo("TestFeature");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => feature.SetMetadata(null!, "value"));
    }

    [Fact]
    public void GetMetadata_ReturnsValue()
    {
        // Arrange
        var feature = new FeatureInfo("TestFeature");
        feature.SetMetadata("key1", "value1");

        // Act
        var result = feature.GetMetadata("key1");

        // Assert
        result.Should().Be("value1");
    }

    [Fact]
    public void GetMetadata_WithMissingKey_ReturnsDefaultValue()
    {
        // Arrange
        var feature = new FeatureInfo("TestFeature");

        // Act
        var result = feature.GetMetadata("missing", "default");

        // Assert
        result.Should().Be("default");
    }

    [Fact]
    public void Clone_CreatesIndependentCopy()
    {
        // Arrange
        var feature = new FeatureInfo("TestFeature")
        {
            Enabled = true,
            Description = "Test Description"
        };
        feature.SetMetadata("key1", "value1");

        // Act
        var clone = feature.Clone();

        // Assert
        clone.Should().NotBeSameAs(feature);
        clone.Name.Should().Be(feature.Name);
        clone.Enabled.Should().Be(feature.Enabled);
        clone.Description.Should().Be(feature.Description);
        clone.Metadata.Should().Equal(feature.Metadata);

        // Modify original and verify clone is independent
        feature.Enabled = false;
        clone.Enabled.Should().BeTrue();
    }

    [Fact]
    public void Metadata_IsCaseInsensitive()
    {
        // Arrange
        var feature = new FeatureInfo("TestFeature");
        feature.SetMetadata("Key1", "value1");

        // Act & Assert
        feature.GetMetadata("key1").Should().Be("value1");
        feature.GetMetadata("KEY1").Should().Be("value1");
    }
}
