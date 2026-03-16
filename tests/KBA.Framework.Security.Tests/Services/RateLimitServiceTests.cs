using KBA.Framework.Security.Interfaces;
using KBA.Framework.Security.Models;
using KBA.Framework.Security.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace KBA.Framework.Security.Tests.Services;

/// <summary>
/// Tests pour RateLimitService
/// </summary>
public class RateLimitServiceTests
{
    private readonly Mock<IRateLimitStore> _mockStore;
    private readonly Mock<ILogger<RateLimitService>> _mockLogger;
    private readonly Mock<IOptions<RateLimitOptions>> _mockOptions;
    private readonly EndpointRuleCache _ruleCache;
    private readonly RateLimitService _service;
    private readonly RateLimitOptions _options;

    public RateLimitServiceTests()
    {
        _mockStore = new Mock<IRateLimitStore>();
        _mockLogger = new Mock<ILogger<RateLimitService>>();
        _mockOptions = new Mock<IOptions<RateLimitOptions>>();

        // Configuration par défaut des options
        _options = new RateLimitOptions
        {
            Enabled = true,
            Rules = new List<RateLimitRule>
            {
                new RateLimitRule
                {
                    Name = "GlobalRule",
                    EndpointPattern = null,
                    Strategy = RateLimitStrategy.FixedWindow,
                    Limit = 100,
                    WindowSeconds = 60,
                    KeyType = RateLimitKeyType.IpAddress,
                    Order = 10,
                    Enabled = true
                },
                new RateLimitRule
                {
                    Name = "ApiRule",
                    EndpointPattern = "/api/*",
                    Strategy = RateLimitStrategy.SlidingWindow,
                    Limit = 50,
                    WindowSeconds = 60,
                    KeyType = RateLimitKeyType.IpAddress,
                    Order = 5,
                    Enabled = true
                },
                new RateLimitRule
                {
                    Name = "OrdersRule",
                    EndpointPattern = "/api/orders",
                    Strategy = RateLimitStrategy.TokenBucket,
                    Limit = 20,
                    WindowSeconds = 60,
                    BucketCapacity = 20,
                    RefillRate = 0.5,
                    KeyType = RateLimitKeyType.UserId,
                    Order = 1,
                    Enabled = true
                },
                new RateLimitRule
                {
                    Name = "DisabledRule",
                    EndpointPattern = "/api/disabled",
                    Strategy = RateLimitStrategy.FixedWindow,
                    Limit = 10,
                    WindowSeconds = 60,
                    KeyType = RateLimitKeyType.IpAddress,
                    Order = 1,
                    Enabled = false
                }
            }
        };

        _mockOptions.Setup(x => x.Value).Returns(_options);

        // Create cache with the same options
        _ruleCache = new EndpointRuleCache(_mockOptions.Object);

        _service = new RateLimitService(_mockStore.Object, _mockOptions.Object, _mockLogger.Object, _ruleCache);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenStoreIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new RateLimitService(null!, _mockOptions.Object, _mockLogger.Object, _ruleCache));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenOptionsIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new RateLimitService(_mockStore.Object, null!, _mockLogger.Object, _ruleCache));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new RateLimitService(_mockStore.Object, _mockOptions.Object, null!, _ruleCache));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenRuleCacheIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new RateLimitService(_mockStore.Object, _mockOptions.Object, _mockLogger.Object, null!));
    }

    #endregion

    #region CheckAsync (Single Rule) Tests

    [Fact]
    public async Task CheckAsync_ShouldThrowArgumentException_WhenKeyIsEmpty()
    {
        // Arrange
        var rule = _options.Rules[0];

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CheckAsync(string.Empty, rule));
    }

    [Fact]
    public async Task CheckAsync_ShouldThrowArgumentNullException_WhenRuleIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.CheckAsync("test-key", null!));
    }

    [Fact]
    public async Task CheckAsync_ShouldReturnAllowed_WhenRuleIsDisabled()
    {
        // Arrange
        var rule = new RateLimitRule
        {
            Name = "DisabledTest",
            Enabled = false,
            Limit = 10,
            WindowSeconds = 60
        };

        // Act
        var result = await _service.CheckAsync("test-key", rule);

        // Assert
        Assert.True(result.IsAllowed);
        Assert.Equal(10, result.Remaining);
        Assert.Equal(10, result.Limit);
        Assert.Equal("DisabledTest", result.RuleName);
        _mockStore.Verify(x => x.IncrementAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckAsync_FixedWindow_ShouldReturnAllowed_WhenUnderLimit()
    {
        // Arrange
        var rule = new RateLimitRule
        {
            Name = "FixedWindowTest",
            Strategy = RateLimitStrategy.FixedWindow,
            Limit = 10,
            WindowSeconds = 60,
            Enabled = true
        };

        _mockStore.Setup(x => x.IncrementAsync("FixedWindowTest:test-key", 60, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act
        var result = await _service.CheckAsync("test-key", rule);

        // Assert
        Assert.True(result.IsAllowed);
        Assert.Equal(5, result.Remaining); // 10 - 5 = 5
        Assert.Equal(10, result.Limit);
        Assert.Equal("FixedWindowTest", result.RuleName);
    }

    [Fact]
    public async Task CheckAsync_FixedWindow_ShouldReturnDenied_WhenOverLimit()
    {
        // Arrange
        var rule = new RateLimitRule
        {
            Name = "FixedWindowTest",
            Strategy = RateLimitStrategy.FixedWindow,
            Limit = 10,
            WindowSeconds = 60,
            Enabled = true
        };

        _mockStore.Setup(x => x.IncrementAsync("FixedWindowTest:test-key", 60, It.IsAny<CancellationToken>()))
            .ReturnsAsync(11);

        // Act
        var result = await _service.CheckAsync("test-key", rule);

        // Assert
        Assert.False(result.IsAllowed);
        Assert.Equal(0, result.Remaining);
        Assert.Equal(10, result.Limit);
        Assert.Equal(60, result.RetryAfterSeconds);
        Assert.Equal("FixedWindowTest", result.RuleName);
    }

    [Fact]
    public async Task CheckAsync_SlidingWindow_ShouldReturnAllowed_WhenNewWindow()
    {
        // Arrange
        var rule = new RateLimitRule
        {
            Name = "SlidingWindowTest",
            Strategy = RateLimitStrategy.SlidingWindow,
            Limit = 10,
            WindowSeconds = 60,
            Enabled = true
        };

        _mockStore.Setup(x => x.GetTimestampAsync("SlidingWindowTest:test-key:ts", It.IsAny<CancellationToken>()))
            .ReturnsAsync((DateTime?)null);

        _mockStore.Setup(x => x.SetAsync("SlidingWindowTest:test-key", 1, 60, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockStore.Setup(x => x.SetTimestampAsync("SlidingWindowTest:test-key:ts", It.IsAny<DateTime>(), 60, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CheckAsync("test-key", rule);

        // Assert
        Assert.True(result.IsAllowed);
        Assert.Equal(9, result.Remaining); // 10 - 1 = 9
        Assert.Equal(10, result.Limit);
        Assert.Equal("SlidingWindowTest", result.RuleName);
    }

    [Fact]
    public async Task CheckAsync_SlidingWindow_ShouldReturnAllowed_WhenWithinWindow()
    {
        // Arrange
        var rule = new RateLimitRule
        {
            Name = "SlidingWindowTest",
            Strategy = RateLimitStrategy.SlidingWindow,
            Limit = 10,
            WindowSeconds = 60,
            Enabled = true
        };

        _mockStore.Setup(x => x.GetTimestampAsync("SlidingWindowTest:test-key:ts", It.IsAny<CancellationToken>()))
            .ReturnsAsync(DateTime.UtcNow.AddSeconds(-30));

        _mockStore.Setup(x => x.IncrementAsync("SlidingWindowTest:test-key", 60, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act
        var result = await _service.CheckAsync("test-key", rule);

        // Assert
        Assert.True(result.IsAllowed);
        Assert.Equal(5, result.Remaining);
        Assert.Equal(10, result.Limit);
    }

    [Fact]
    public async Task CheckAsync_SlidingWindow_ShouldReturnDenied_WhenOverLimit()
    {
        // Arrange
        var rule = new RateLimitRule
        {
            Name = "SlidingWindowTest",
            Strategy = RateLimitStrategy.SlidingWindow,
            Limit = 10,
            WindowSeconds = 60,
            Enabled = true
        };

        _mockStore.Setup(x => x.GetTimestampAsync("SlidingWindowTest:test-key:ts", It.IsAny<CancellationToken>()))
            .ReturnsAsync(DateTime.UtcNow.AddSeconds(-30));

        _mockStore.Setup(x => x.IncrementAsync("SlidingWindowTest:test-key", 60, It.IsAny<CancellationToken>()))
            .ReturnsAsync(11);

        // Act
        var result = await _service.CheckAsync("test-key", rule);

        // Assert
        Assert.False(result.IsAllowed);
        Assert.Equal(0, result.Remaining);
        Assert.Equal(10, result.Limit);
    }

    [Fact]
    public async Task CheckAsync_TokenBucket_ShouldReturnAllowed_WhenTokensAvailable()
    {
        // Arrange
        var rule = new RateLimitRule
        {
            Name = "TokenBucketTest",
            Strategy = RateLimitStrategy.TokenBucket,
            Limit = 10,
            BucketCapacity = 10,
            RefillRate = 1.0,
            WindowSeconds = 60,
            Enabled = true
        };

        _mockStore.Setup(x => x.GetTokenCountAsync("TokenBucketTest:test-key:bucket", It.IsAny<CancellationToken>()))
            .ReturnsAsync(5.0);

        _mockStore.Setup(x => x.SetTokenBucketAsync("TokenBucketTest:test-key:bucket", 4.0, It.IsAny<DateTime>(), 60, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CheckAsync("test-key", rule);

        // Assert
        Assert.True(result.IsAllowed);
        Assert.Equal(4, result.Remaining);
        Assert.Equal(10, result.Limit);
    }

    [Fact]
    public async Task CheckAsync_TokenBucket_ShouldReturnDenied_WhenNoTokensAvailable()
    {
        // Arrange
        var rule = new RateLimitRule
        {
            Name = "TokenBucketTest",
            Strategy = RateLimitStrategy.TokenBucket,
            Limit = 10,
            BucketCapacity = 10,
            RefillRate = 0.5,
            WindowSeconds = 60,
            Enabled = true
        };

        _mockStore.Setup(x => x.GetTokenCountAsync("TokenBucketTest:test-key:bucket", It.IsAny<CancellationToken>()))
            .ReturnsAsync(0.5);

        // Act
        var result = await _service.CheckAsync("test-key", rule);

        // Assert
        Assert.False(result.IsAllowed);
        Assert.Equal(0, result.Remaining);
        Assert.Equal(10, result.Limit);
        Assert.Equal(2, result.RetryAfterSeconds); // Math.Ceiling(1.0 / 0.5) = 2
    }

    #endregion

    #region CheckAsync (Endpoint-based) Tests - Cache Integration

    [Fact]
    public async Task CheckAsync_ShouldReturnAllowed_WhenNoRulesApply()
    {
        // Arrange
        var endpoint = "/nonexistent/endpoint";

        // Act
        var result = await _service.CheckAsync(endpoint, "192.168.1.1", null, null);

        // Assert
        Assert.True(result.IsAllowed);
        Assert.Equal(long.MaxValue, result.Remaining);
        Assert.Equal(long.MaxValue, result.Limit);
        Assert.Null(result.RuleName);
        _mockStore.Verify(x => x.IncrementAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckAsync_ShouldUseCache_WhenRetrievingRules()
    {
        // Arrange
        var endpoint = "/api/orders";

        _mockStore.Setup(x => x.GetTokenCountAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(10.0);

        _mockStore.Setup(x => x.SetTokenBucketAsync(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockStore.Setup(x => x.IncrementAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.CheckAsync(endpoint, "192.168.1.1", "user123", null);

        // Assert
        Assert.True(result.IsAllowed);
        // Verify that rules were retrieved from cache (OrdersRule has highest priority with Order = 1)
        Assert.Equal("OrdersRule", result.RuleName);
    }

    [Fact]
    public async Task CheckAsync_ShouldReturnFirstDenial_WhenMultipleRulesApply()
    {
        // Arrange
        var endpoint = "/api/orders";

        // First rule (OrdersRule - TokenBucket) will be checked first (Order = 1)
        _mockStore.Setup(x => x.GetTokenCountAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0.0); // No tokens, should deny

        // Act
        var result = await _service.CheckAsync(endpoint, "192.168.1.1", "user123", null);

        // Assert
        Assert.False(result.IsAllowed);
        Assert.Equal("OrdersRule", result.RuleName);
        // Should not have checked other rules
        _mockStore.Verify(x => x.IncrementAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckAsync_ShouldCheckAllRules_WhenAllPass()
    {
        // Arrange
        var endpoint = "/api/products";

        // Setup mocks for ApiRule (wildcard) and GlobalRule
        _mockStore.Setup(x => x.GetTimestampAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DateTime.UtcNow.AddSeconds(-30));

        _mockStore.Setup(x => x.IncrementAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act
        var result = await _service.CheckAsync(endpoint, "192.168.1.1", null, null);

        // Assert
        Assert.True(result.IsAllowed);
        // ApiRule should be checked first (Order = 5), then GlobalRule (Order = 10)
        Assert.Equal("ApiRule", result.RuleName);
    }

    [Fact]
    public async Task CheckAsync_ShouldMatchWildcardPattern_FromCache()
    {
        // Arrange
        var endpoint = "/api/users/123";

        _mockStore.Setup(x => x.GetTimestampAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DateTime.UtcNow.AddSeconds(-30));

        _mockStore.Setup(x => x.IncrementAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act
        var result = await _service.CheckAsync(endpoint, "192.168.1.1", null, null);

        // Assert
        Assert.True(result.IsAllowed);
        // Should match ApiRule with wildcard pattern "/api/*"
        Assert.Equal("ApiRule", result.RuleName);
    }

    [Fact]
    public async Task CheckAsync_ShouldMatchGlobalRule_FromCache()
    {
        // Arrange
        var endpoint = "/public/health";

        _mockStore.Setup(x => x.IncrementAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act
        var result = await _service.CheckAsync(endpoint, "192.168.1.1", null, null);

        // Assert
        Assert.True(result.IsAllowed);
        // Should match GlobalRule (no pattern = matches all)
        Assert.Equal("GlobalRule", result.RuleName);
    }

    #endregion

    #region BuildKey Tests

    [Fact]
    public void BuildKey_ShouldReturnIpAddress_WhenKeyTypeIsIpAddress()
    {
        // Arrange
        var rule = new RateLimitRule { KeyType = RateLimitKeyType.IpAddress };

        // Act
        var key = _service.BuildKey(rule, "/api/test", "192.168.1.1", "user123", "tenant1");

        // Assert
        Assert.Equal("192.168.1.1", key);
    }

    [Fact]
    public void BuildKey_ShouldReturnAnonymous_WhenKeyTypeIsIpAddressAndIpIsNull()
    {
        // Arrange
        var rule = new RateLimitRule { KeyType = RateLimitKeyType.IpAddress };

        // Act
        var key = _service.BuildKey(rule, "/api/test", null, "user123", "tenant1");

        // Assert
        Assert.Equal("anonymous", key);
    }

    [Fact]
    public void BuildKey_ShouldReturnUserId_WhenKeyTypeIsUserId()
    {
        // Arrange
        var rule = new RateLimitRule { KeyType = RateLimitKeyType.UserId };

        // Act
        var key = _service.BuildKey(rule, "/api/test", "192.168.1.1", "user123", "tenant1");

        // Assert
        Assert.Equal("user123", key);
    }

    [Fact]
    public void BuildKey_ShouldReturnTenantId_WhenKeyTypeIsTenantId()
    {
        // Arrange
        var rule = new RateLimitRule { KeyType = RateLimitKeyType.TenantId };

        // Act
        var key = _service.BuildKey(rule, "/api/test", "192.168.1.1", "user123", "tenant1");

        // Assert
        Assert.Equal("tenant1", key);
    }

    [Fact]
    public void BuildKey_ShouldReturnDefault_WhenKeyTypeIsTenantIdAndTenantIsNull()
    {
        // Arrange
        var rule = new RateLimitRule { KeyType = RateLimitKeyType.TenantId };

        // Act
        var key = _service.BuildKey(rule, "/api/test", "192.168.1.1", "user123", null);

        // Assert
        Assert.Equal("default", key);
    }

    [Fact]
    public void BuildKey_ShouldReturnEndpoint_WhenKeyTypeIsEndpoint()
    {
        // Arrange
        var rule = new RateLimitRule { KeyType = RateLimitKeyType.Endpoint };

        // Act
        var key = _service.BuildKey(rule, "/api/test", "192.168.1.1", "user123", "tenant1");

        // Assert
        Assert.Equal("/api/test", key);
    }

    [Fact]
    public void BuildKey_ShouldReturnIpAndEndpoint_WhenKeyTypeIsIpAndEndpoint()
    {
        // Arrange
        var rule = new RateLimitRule { KeyType = RateLimitKeyType.IpAndEndpoint };

        // Act
        var key = _service.BuildKey(rule, "/api/test", "192.168.1.1", "user123", "tenant1");

        // Assert
        Assert.Equal("192.168.1.1:/api/test", key);
    }

    [Fact]
    public void BuildKey_ShouldReturnUserAndEndpoint_WhenKeyTypeIsUserAndEndpoint()
    {
        // Arrange
        var rule = new RateLimitRule { KeyType = RateLimitKeyType.UserAndEndpoint };

        // Act
        var key = _service.BuildKey(rule, "/api/test", "192.168.1.1", "user123", "tenant1");

        // Assert
        Assert.Equal("user123:/api/test", key);
    }

    [Fact]
    public void BuildKey_ShouldReturnCustomKey_WhenKeyTypeIsCustom()
    {
        // Arrange
        var rule = new RateLimitRule
        {
            KeyType = RateLimitKeyType.Custom,
            CustomKeySelector = "customSelector"
        };

        // Act
        var key = _service.BuildKey(rule, "/api/test", "192.168.1.1", "user123", "tenant1");

        // Assert
        Assert.Equal("customSelector:/api/test", key);
    }

    #endregion
}
