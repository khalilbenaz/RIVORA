namespace KBA.Framework.Features.Tests.Core;

using KBA.Framework.Features.Core;
using KBA.Framework.Features.Providers;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for the <see cref="FeatureManager"/> class.
/// </summary>
public class FeatureManagerTests
{
    private readonly Mock<IFeatureProvider> _configProviderMock;
    private readonly Mock<IFeatureProvider> _databaseProviderMock;
    private readonly FeatureManagerOptions _managerOptions;

    public FeatureManagerTests()
    {
        _configProviderMock = new Mock<IFeatureProvider>();
        _configProviderMock.Setup(x => x.ProviderType).Returns(FeatureProvider.Config);
        
        _databaseProviderMock = new Mock<IFeatureProvider>();
        _databaseProviderMock.Setup(x => x.ProviderType).Returns(FeatureProvider.Database);

        _managerOptions = new FeatureManagerOptions
        {
            UseFirstProvider = true,
            DefaultEnabledState = false
        };
    }

    [Fact]
    public void IsEnabled_DatabaseProviderHasFeature_UsesDatabaseProvider()
    {
        // Arrange
        _databaseProviderMock.Setup(x => x.IsEnabled("Feature1")).Returns(true);
        _configProviderMock.Setup(x => x.IsEnabled("Feature1")).Returns(false);
        
        var manager = CreateManager(_databaseProviderMock.Object, _configProviderMock.Object);

        // Act
        var result = manager.IsEnabled("Feature1");

        // Assert
        result.Should().BeTrue();
        _databaseProviderMock.Verify(x => x.IsEnabled("Feature1"), Times.Once);
    }

    [Fact]
    public void IsEnabled_NoProviderHasFeature_ReturnsDefaultState()
    {
        // Arrange
        _databaseProviderMock.Setup(x => x.IsEnabled("Feature1")).Returns(false);
        _configProviderMock.Setup(x => x.IsEnabled("Feature1")).Returns(false);
        
        var manager = CreateManager(_databaseProviderMock.Object, _configProviderMock.Object);

        // Act
        var result = manager.IsEnabled("Feature1");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsEnabled_EmptyFeatureName_ReturnsFalse()
    {
        // Arrange
        var manager = CreateManager();

        // Act
        var result = manager.IsEnabled("");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetFeature_DatabaseProviderHasFeature_ReturnsFromDatabase()
    {
        // Arrange
        var dbFeature = new FeatureInfo("Feature1") { Enabled = true };
        _databaseProviderMock.Setup(x => x.GetFeature("Feature1")).Returns(dbFeature);
        
        var manager = CreateManager(_databaseProviderMock.Object);

        // Act
        var result = manager.GetFeature("Feature1");

        // Assert
        result.Should().NotBeNull();
        result!.Enabled.Should().BeTrue();
    }

    [Fact]
    public void GetFeature_NoProviderHasFeature_ReturnsNull()
    {
        // Arrange
        _databaseProviderMock.Setup(x => x.GetFeature("Feature1")).Returns((FeatureInfo?)null);
        _configProviderMock.Setup(x => x.GetFeature("Feature1")).Returns((FeatureInfo?)null);
        
        var manager = CreateManager(_databaseProviderMock.Object, _configProviderMock.Object);

        // Act
        var result = manager.GetFeature("Feature1");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void SetEnabled_DatabaseProvider_UpdatesFeature()
    {
        // Arrange
        var dbFeature = new FeatureInfo("Feature1") { Enabled = true };
        _databaseProviderMock.Setup(x => x.GetFeature("Feature1")).Returns(dbFeature);
        _databaseProviderMock.Setup(x => x.SetEnabled("Feature1", false)).Returns(true);
        
        var manager = CreateManager(_databaseProviderMock.Object);

        // Act
        var result = manager.SetEnabled("Feature1", false);

        // Assert
        result.Should().BeTrue();
        _databaseProviderMock.Verify(x => x.SetEnabled("Feature1", false), Times.Once);
    }

    [Fact]
    public void SetEnabled_ConfigProviderOnly_ReturnsFalse()
    {
        // Arrange
        var configFeature = new FeatureInfo("Feature1") { Enabled = true };
        _configProviderMock.Setup(x => x.GetFeature("Feature1")).Returns(configFeature);
        _configProviderMock.Setup(x => x.SetEnabled("Feature1", false)).Returns(false);
        
        var manager = CreateManager(_configProviderMock.Object);

        // Act
        var result = manager.SetEnabled("Feature1", false);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetAllFeatures_MergesAllProviders()
    {
        // Arrange
        var dbFeatures = new List<FeatureInfo>
        {
            new FeatureInfo("DbFeature1") { Enabled = true },
            new FeatureInfo("DbFeature2") { Enabled = false }
        };
        var configFeatures = new List<FeatureInfo>
        {
            new FeatureInfo("ConfigFeature1") { Enabled = true }
        };
        
        _databaseProviderMock.Setup(x => x.GetAllFeatures()).Returns(dbFeatures);
        _configProviderMock.Setup(x => x.GetAllFeatures()).Returns(configFeatures);
        
        var manager = CreateManager(_databaseProviderMock.Object, _configProviderMock.Object);

        // Act
        var result = manager.GetAllFeatures().ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(f => f.Name == "DbFeature1");
        result.Should().Contain(f => f.Name == "DbFeature2");
        result.Should().Contain(f => f.Name == "ConfigFeature1");
    }

    [Fact]
    public void GetAllFeatures_DuplicateFeatures_UsesFirstProvider()
    {
        // Arrange
        var dbFeature = new FeatureInfo("SharedFeature") { Enabled = true, Provider = FeatureProvider.Database };
        var configFeature = new FeatureInfo("SharedFeature") { Enabled = false, Provider = FeatureProvider.Config };
        
        _databaseProviderMock.Setup(x => x.GetAllFeatures()).Returns(new[] { dbFeature });
        _configProviderMock.Setup(x => x.GetAllFeatures()).Returns(new[] { configFeature });
        
        var manager = CreateManager(_databaseProviderMock.Object, _configProviderMock.Object);

        // Act
        var result = manager.GetAllFeatures().ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Provider.Should().Be(FeatureProvider.Database);
    }

    [Fact]
    public void Provider_Exception_IsLoggedAndContinues()
    {
        // Arrange
        _databaseProviderMock.Setup(x => x.IsEnabled("Feature1")).Throws(new InvalidOperationException("Test error"));
        _configProviderMock.Setup(x => x.IsEnabled("Feature1")).Returns(true);
        
        var manager = CreateManager(_databaseProviderMock.Object, _configProviderMock.Object);

        // Act
        var result = manager.IsEnabled("Feature1");

        // Assert
        result.Should().BeTrue();
    }

    private FeatureManager CreateManager(params IFeatureProvider[] providers)
    {
        return new FeatureManager(providers, _managerOptions, NullLogger<FeatureManager>.Instance);
    }
}
