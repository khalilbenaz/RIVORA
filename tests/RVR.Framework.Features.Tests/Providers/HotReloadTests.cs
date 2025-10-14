namespace RVR.Framework.Features.Tests.Providers;

using RVR.Framework.Features.Core;
using RVR.Framework.Features.Providers;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

/// <summary>
/// Integration tests for hot-reload functionality.
/// </summary>
public class HotReloadTests : IDisposable
{
    private readonly string _testConfigPath;
    private readonly FeatureFlagsOptions _options;
    private readonly Mock<IOptionsMonitor<FeatureFlagsOptions>> _optionsMonitorMock;

    public HotReloadTests()
    {
        _testConfigPath = Path.Combine(Path.GetTempPath(), $"test_features_{Guid.NewGuid()}.json");
        
        _options = new FeatureFlagsOptions
        {
            EnableHotReload = true,
            ConfigFilePath = _testConfigPath,
            Features = new List<FeatureConfig>
            {
                new FeatureConfig { Name = "InitialFeature", Enabled = true }
            }
        };

        _optionsMonitorMock = new Mock<IOptionsMonitor<FeatureFlagsOptions>>();
        _optionsMonitorMock.Setup(x => x.CurrentValue).Returns(_options);
    }

    [Fact]
    public void ConfigFeatureProvider_InitializesWithFeatures()
    {
        // Arrange & Act
        using var provider = new ConfigFeatureProvider(_optionsMonitorMock.Object, NullLogger<ConfigFeatureProvider>.Instance);

        // Assert
        provider.IsEnabled("InitialFeature").Should().BeTrue();
    }

    [Fact]
    public void ConfigFeatureProvider_OnChangeCallback_ReloadsFeatures()
    {
        // Arrange - Capture the OnChange callback before creating the provider
        Action<FeatureFlagsOptions, string?>? capturedCallback = null;
        _optionsMonitorMock.Setup(x => x.OnChange(It.IsAny<Action<FeatureFlagsOptions, string?>>()))
            .Callback<Action<FeatureFlagsOptions, string?>>(callback => capturedCallback = callback)
            .Returns(Mock.Of<IDisposable>());

        using var provider = new ConfigFeatureProvider(_optionsMonitorMock.Object, NullLogger<ConfigFeatureProvider>.Instance);
        provider.IsEnabled("InitialFeature").Should().BeTrue();

        // Modify options
        _options.Features.Clear();
        _options.Features.Add(new FeatureConfig { Name = "NewFeature", Enabled = true });

        // Act - Trigger the captured callback to simulate configuration change
        capturedCallback.Should().NotBeNull("the provider should register an OnChange callback");
        capturedCallback!(_options, null);

        // Assert
        provider.IsEnabled("NewFeature").Should().BeTrue();
        provider.IsEnabled("InitialFeature").Should().BeFalse();
    }

    [Fact]
    public void ConfigFeatureProvider_GetAllFeatures_ReturnsCurrentFeatures()
    {
        // Arrange
        using var provider = new ConfigFeatureProvider(_optionsMonitorMock.Object, NullLogger<ConfigFeatureProvider>.Instance);

        // Act
        var features = provider.GetAllFeatures().ToList();

        // Assert
        features.Should().HaveCount(1);
        features[0].Name.Should().Be("InitialFeature");
    }

    [Fact]
    public async Task ConfigFeatureProvider_IsEnabledAsync_ThreadSafe()
    {
        // Arrange
        using var provider = new ConfigFeatureProvider(_optionsMonitorMock.Object, NullLogger<ConfigFeatureProvider>.Instance);
        var tasks = new List<Task<bool>>();

        // Act - Run multiple concurrent checks
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(provider.IsEnabledAsync("InitialFeature"));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllSatisfy(r => r.Should().BeTrue());
    }

    [Fact]
    public void ConfigFeatureProvider_Dispose_CleansUpResources()
    {
        // Arrange
        var provider = new ConfigFeatureProvider(_optionsMonitorMock.Object, NullLogger<ConfigFeatureProvider>.Instance);

        // Act & Assert - Dispose should not throw
        var act = () => provider.Dispose();
        act.Should().NotThrow();

        // Calling Dispose again should also not throw (idempotent)
        act.Should().NotThrow();
    }

    [Fact]
    public void FeatureInfo_Clone_IsThreadSafe()
    {
        // Arrange
        var feature = new FeatureInfo("TestFeature")
        {
            Enabled = true,
            Description = "Test"
        };
        feature.SetMetadata("key1", "value1");

        var clones = new List<FeatureInfo>();
        var tasks = new List<Task>();

        // Act - Create clones concurrently
        for (int i = 0; i < 50; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                lock (clones)
                {
                    clones.Add(feature.Clone());
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert
        clones.Should().HaveCount(50);
        clones.Should().AllSatisfy(c =>
        {
            c.Name.Should().Be("TestFeature");
            c.Enabled.Should().BeTrue();
        });
    }

    public void Dispose()
    {
        if (File.Exists(_testConfigPath))
        {
            File.Delete(_testConfigPath);
        }
    }
}
