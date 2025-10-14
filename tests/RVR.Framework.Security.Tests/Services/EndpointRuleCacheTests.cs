using RVR.Framework.Security.Models;
using RVR.Framework.Security.Services;
using Microsoft.Extensions.Options;

namespace RVR.Framework.Security.Tests.Services;

/// <summary>
/// Tests pour EndpointRuleCache
/// </summary>
public class EndpointRuleCacheTests
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenOptionsIsNull()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new EndpointRuleCache(null!));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenOptionsValueIsNull()
    {
        // Arrange
        var options = Options.Create<RateLimitOptions>(null!);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new EndpointRuleCache(options));
    }

    [Fact]
    public void GetRulesForEndpoint_ShouldReturnEmptyList_WhenEndpointIsNull()
    {
        // Arrange
        var options = CreateOptions(new List<RateLimitRule>());
        var cache = new EndpointRuleCache(options);

        // Act
        var result = cache.GetRulesForEndpoint(null!);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetRulesForEndpoint_ShouldReturnEmptyList_WhenEndpointIsEmpty()
    {
        // Arrange
        var options = CreateOptions(new List<RateLimitRule>());
        var cache = new EndpointRuleCache(options);

        // Act
        var result = cache.GetRulesForEndpoint(string.Empty);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetRulesForEndpoint_ShouldReturnEmptyList_WhenEndpointIsWhitespace()
    {
        // Arrange
        var options = CreateOptions(new List<RateLimitRule>());
        var cache = new EndpointRuleCache(options);

        // Act
        var result = cache.GetRulesForEndpoint("   ");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetRulesForEndpoint_ShouldReturnExactMatchRule_WhenPatternMatchesExactly()
    {
        // Arrange
        var rules = new List<RateLimitRule>
        {
            new RateLimitRule
            {
                Name = "Exact Rule",
                EndpointPattern = "/api/products",
                Enabled = true,
                Order = 1
            }
        };
        var options = CreateOptions(rules);
        var cache = new EndpointRuleCache(options);

        // Act
        var result = cache.GetRulesForEndpoint("/api/products");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Exact Rule", result[0].Name);
    }

    [Fact]
    public void GetRulesForEndpoint_ShouldReturnEmptyList_WhenNoPatternMatches()
    {
        // Arrange
        var rules = new List<RateLimitRule>
        {
            new RateLimitRule
            {
                Name = "Exact Rule",
                EndpointPattern = "/api/products",
                Enabled = true,
                Order = 1
            }
        };
        var options = CreateOptions(rules);
        var cache = new EndpointRuleCache(options);

        // Act
        var result = cache.GetRulesForEndpoint("/api/orders");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetRulesForEndpoint_ShouldReturnWildcardRule_WhenPatternMatchesWithWildcard()
    {
        // Arrange
        var rules = new List<RateLimitRule>
        {
            new RateLimitRule
            {
                Name = "Wildcard Rule",
                EndpointPattern = "/api/products/*",
                Enabled = true,
                Order = 1
            }
        };
        var options = CreateOptions(rules);
        var cache = new EndpointRuleCache(options);

        // Act
        var result = cache.GetRulesForEndpoint("/api/products/123");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Wildcard Rule", result[0].Name);
    }

    [Fact]
    public void GetRulesForEndpoint_ShouldReturnWildcardRule_WhenPatternMatchesWithWildcardAtExactPrefix()
    {
        // Arrange
        var rules = new List<RateLimitRule>
        {
            new RateLimitRule
            {
                Name = "Wildcard Rule",
                EndpointPattern = "/api/products/*",
                Enabled = true,
                Order = 1
            }
        };
        var options = CreateOptions(rules);
        var cache = new EndpointRuleCache(options);

        // Act
        var result = cache.GetRulesForEndpoint("/api/products/");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Wildcard Rule", result[0].Name);
    }

    [Fact]
    public void GetRulesForEndpoint_ShouldNotReturnWildcardRule_WhenPatternDoesNotMatch()
    {
        // Arrange
        var rules = new List<RateLimitRule>
        {
            new RateLimitRule
            {
                Name = "Wildcard Rule",
                EndpointPattern = "/api/products/*",
                Enabled = true,
                Order = 1
            }
        };
        var options = CreateOptions(rules);
        var cache = new EndpointRuleCache(options);

        // Act
        var result = cache.GetRulesForEndpoint("/api/orders/123");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetRulesForEndpoint_ShouldReturnGlobalRule_ForAnyEndpoint()
    {
        // Arrange
        var rules = new List<RateLimitRule>
        {
            new RateLimitRule
            {
                Name = "Global Rule",
                EndpointPattern = null,
                Enabled = true,
                Order = 1
            }
        };
        var options = CreateOptions(rules);
        var cache = new EndpointRuleCache(options);

        // Act
        var result1 = cache.GetRulesForEndpoint("/api/products");
        var result2 = cache.GetRulesForEndpoint("/api/orders");
        var result3 = cache.GetRulesForEndpoint("/anything");

        // Assert
        Assert.NotNull(result1);
        Assert.Single(result1);
        Assert.Equal("Global Rule", result1[0].Name);

        Assert.NotNull(result2);
        Assert.Single(result2);
        Assert.Equal("Global Rule", result2[0].Name);

        Assert.NotNull(result3);
        Assert.Single(result3);
        Assert.Equal("Global Rule", result3[0].Name);
    }

    [Fact]
    public void GetRulesForEndpoint_ShouldReturnGlobalRule_WhenEndpointPatternIsEmpty()
    {
        // Arrange
        var rules = new List<RateLimitRule>
        {
            new RateLimitRule
            {
                Name = "Global Rule",
                EndpointPattern = string.Empty,
                Enabled = true,
                Order = 1
            }
        };
        var options = CreateOptions(rules);
        var cache = new EndpointRuleCache(options);

        // Act
        var result = cache.GetRulesForEndpoint("/api/products");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Global Rule", result[0].Name);
    }

    [Fact]
    public void GetRulesForEndpoint_ShouldNotReturnDisabledRules()
    {
        // Arrange
        var rules = new List<RateLimitRule>
        {
            new RateLimitRule
            {
                Name = "Disabled Rule",
                EndpointPattern = "/api/products",
                Enabled = false,
                Order = 1
            },
            new RateLimitRule
            {
                Name = "Enabled Rule",
                EndpointPattern = "/api/products",
                Enabled = true,
                Order = 2
            }
        };
        var options = CreateOptions(rules);
        var cache = new EndpointRuleCache(options);

        // Act
        var result = cache.GetRulesForEndpoint("/api/products");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Enabled Rule", result[0].Name);
    }

    [Fact]
    public void GetRulesForEndpoint_ShouldReturnRulesInOrderPriority()
    {
        // Arrange
        var rules = new List<RateLimitRule>
        {
            new RateLimitRule
            {
                Name = "Rule Order 3",
                EndpointPattern = "/api/products",
                Enabled = true,
                Order = 3
            },
            new RateLimitRule
            {
                Name = "Rule Order 1",
                EndpointPattern = "/api/products",
                Enabled = true,
                Order = 1
            },
            new RateLimitRule
            {
                Name = "Rule Order 2",
                EndpointPattern = "/api/products",
                Enabled = true,
                Order = 2
            }
        };
        var options = CreateOptions(rules);
        var cache = new EndpointRuleCache(options);

        // Act
        var result = cache.GetRulesForEndpoint("/api/products");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal("Rule Order 1", result[0].Name);
        Assert.Equal("Rule Order 2", result[1].Name);
        Assert.Equal("Rule Order 3", result[2].Name);
    }

    [Fact]
    public void GetRulesForEndpoint_ShouldBeCaseInsensitive_ForExactMatch()
    {
        // Arrange
        var rules = new List<RateLimitRule>
        {
            new RateLimitRule
            {
                Name = "Case Rule",
                EndpointPattern = "/API/Products",
                Enabled = true,
                Order = 1
            }
        };
        var options = CreateOptions(rules);
        var cache = new EndpointRuleCache(options);

        // Act
        var result = cache.GetRulesForEndpoint("/api/products");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Case Rule", result[0].Name);
    }

    [Fact]
    public void GetRulesForEndpoint_ShouldBeCaseInsensitive_ForWildcardMatch()
    {
        // Arrange
        var rules = new List<RateLimitRule>
        {
            new RateLimitRule
            {
                Name = "Case Wildcard Rule",
                EndpointPattern = "/API/Products/*",
                Enabled = true,
                Order = 1
            }
        };
        var options = CreateOptions(rules);
        var cache = new EndpointRuleCache(options);

        // Act
        var result = cache.GetRulesForEndpoint("/api/products/123");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Case Wildcard Rule", result[0].Name);
    }

    [Fact]
    public void GetRulesForEndpoint_ShouldReturnMultipleMatchingRules()
    {
        // Arrange
        var rules = new List<RateLimitRule>
        {
            new RateLimitRule
            {
                Name = "Global Rule",
                EndpointPattern = null,
                Enabled = true,
                Order = 1
            },
            new RateLimitRule
            {
                Name = "Wildcard Rule",
                EndpointPattern = "/api/*",
                Enabled = true,
                Order = 2
            },
            new RateLimitRule
            {
                Name = "Exact Rule",
                EndpointPattern = "/api/products",
                Enabled = true,
                Order = 3
            }
        };
        var options = CreateOptions(rules);
        var cache = new EndpointRuleCache(options);

        // Act
        var result = cache.GetRulesForEndpoint("/api/products");

        // Assert
        // Cache returns: global rules first, then exact matches, then wildcard matches
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal("Global Rule", result[0].Name);
        Assert.Equal("Exact Rule", result[1].Name);
        Assert.Equal("Wildcard Rule", result[2].Name);
    }

    [Fact]
    public void GetRulesForEndpoint_ShouldHandleMultipleWildcardPatterns()
    {
        // Arrange
        var rules = new List<RateLimitRule>
        {
            new RateLimitRule
            {
                Name = "API Wildcard",
                EndpointPattern = "/api/*",
                Enabled = true,
                Order = 1
            },
            new RateLimitRule
            {
                Name = "API Products Wildcard",
                EndpointPattern = "/api/products/*",
                Enabled = true,
                Order = 2
            }
        };
        var options = CreateOptions(rules);
        var cache = new EndpointRuleCache(options);

        // Act
        var result = cache.GetRulesForEndpoint("/api/products/123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Name == "API Wildcard");
        Assert.Contains(result, r => r.Name == "API Products Wildcard");
    }

    [Fact]
    public void GetRulesForEndpoint_ShouldOnlyMatchLongestWildcardPattern()
    {
        // Arrange
        var rules = new List<RateLimitRule>
        {
            new RateLimitRule
            {
                Name = "Short Wildcard",
                EndpointPattern = "/api/*",
                Enabled = true,
                Order = 1
            },
            new RateLimitRule
            {
                Name = "Long Wildcard",
                EndpointPattern = "/api/products/*",
                Enabled = true,
                Order = 2
            }
        };
        var options = CreateOptions(rules);
        var cache = new EndpointRuleCache(options);

        // Act - Endpoint that matches both patterns
        var result1 = cache.GetRulesForEndpoint("/api/products/123");

        // Act - Endpoint that matches only short pattern
        var result2 = cache.GetRulesForEndpoint("/api/orders");

        // Assert - First endpoint matches both
        Assert.NotNull(result1);
        Assert.Equal(2, result1.Count);
        Assert.Contains(result1, r => r.Name == "Short Wildcard");
        Assert.Contains(result1, r => r.Name == "Long Wildcard");

        // Assert - Second endpoint matches only short
        Assert.NotNull(result2);
        Assert.Single(result2);
        Assert.Equal("Short Wildcard", result2[0].Name);
    }

    [Fact]
    public void GetRulesForEndpoint_ShouldHandleEmptyRulesList()
    {
        // Arrange
        var options = CreateOptions(new List<RateLimitRule>());
        var cache = new EndpointRuleCache(options);

        // Act
        var result = cache.GetRulesForEndpoint("/api/products");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetRulesForEndpoint_ShouldHandleNullRulesList()
    {
        // Arrange
        var rateLimitOptions = new RateLimitOptions
        {
            Rules = null!
        };
        var options = Options.Create(rateLimitOptions);
        var cache = new EndpointRuleCache(options);

        // Act
        var result = cache.GetRulesForEndpoint("/api/products");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetRulesForEndpoint_ShouldCacheRulesEfficiently_ForMultipleCalls()
    {
        // Arrange
        var rules = new List<RateLimitRule>
        {
            new RateLimitRule
            {
                Name = "Exact Rule",
                EndpointPattern = "/api/products",
                Enabled = true,
                Order = 1
            }
        };
        var options = CreateOptions(rules);
        var cache = new EndpointRuleCache(options);

        // Act - Call multiple times with same endpoint
        var result1 = cache.GetRulesForEndpoint("/api/products");
        var result2 = cache.GetRulesForEndpoint("/api/products");
        var result3 = cache.GetRulesForEndpoint("/api/products");

        // Assert - All should return the same rules
        Assert.NotNull(result1);
        Assert.Single(result1);
        Assert.Equal("Exact Rule", result1[0].Name);

        Assert.NotNull(result2);
        Assert.Single(result2);
        Assert.Equal("Exact Rule", result2[0].Name);

        Assert.NotNull(result3);
        Assert.Single(result3);
        Assert.Equal("Exact Rule", result3[0].Name);
    }

    [Fact]
    public void GetRulesForEndpoint_ShouldHandleSpecialCharactersInPattern()
    {
        // Arrange
        var rules = new List<RateLimitRule>
        {
            new RateLimitRule
            {
                Name = "Special Chars Rule",
                EndpointPattern = "/api/products-v2",
                Enabled = true,
                Order = 1
            }
        };
        var options = CreateOptions(rules);
        var cache = new EndpointRuleCache(options);

        // Act
        var result = cache.GetRulesForEndpoint("/api/products-v2");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Special Chars Rule", result[0].Name);
    }

    /// <summary>
    /// Helper method to create IOptions{RateLimitOptions} for testing.
    /// </summary>
    /// <param name="rules">The list of rules to include.</param>
    /// <returns>An IOptions instance.</returns>
    private static IOptions<RateLimitOptions> CreateOptions(List<RateLimitRule> rules)
    {
        var rateLimitOptions = new RateLimitOptions
        {
            Rules = rules
        };
        return Options.Create(rateLimitOptions);
    }
}
