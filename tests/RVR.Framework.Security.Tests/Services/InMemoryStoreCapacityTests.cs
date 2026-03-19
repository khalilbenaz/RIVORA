using FluentAssertions;
using RVR.Framework.Security.Entities;
using RVR.Framework.Security.Services;

namespace RVR.Framework.Security.Tests.Services;

/// <summary>
/// Tests for the bounded capacity behavior of InMemoryRefreshTokenStore.
/// Validates that the store enforces its 10,000-token limit, evicts expired
/// tokens when at capacity, and throws when truly full with no expired tokens.
/// </summary>
public class InMemoryStoreCapacityTests : IDisposable
{
    private readonly InMemoryRefreshTokenStore _store;

    public InMemoryStoreCapacityTests()
    {
        _store = new InMemoryRefreshTokenStore();
    }

    #region Helper Methods

    private static RefreshToken CreateActiveToken(string userId = "user1", int expiresInMinutes = 60)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = Guid.NewGuid().ToString("N"),
            ExpiresUtc = DateTime.UtcNow.AddMinutes(expiresInMinutes),
            CreatedUtc = DateTime.UtcNow
        };
    }

    private static RefreshToken CreateExpiredToken(string userId = "user1")
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = Guid.NewGuid().ToString("N"),
            ExpiresUtc = DateTime.UtcNow.AddMinutes(-1),
            CreatedUtc = DateTime.UtcNow.AddMinutes(-61)
        };
    }

    #endregion

    #region Basic Store Operations

    [Fact]
    public async Task StoreAsync_ShouldAcceptToken_WhenUnderCapacity()
    {
        // Arrange
        var token = CreateActiveToken();

        // Act
        await _store.StoreAsync(token);

        // Assert
        var retrieved = await _store.GetByTokenAsync(token.Token);
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(token.Id);
    }

    [Fact]
    public async Task StoreAsync_ShouldThrow_WhenTokenIsNull()
    {
        // Act
        var act = () => _store.StoreAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task StoreAsync_ShouldAcceptMultipleTokens()
    {
        // Arrange & Act
        var tokens = new List<RefreshToken>();
        for (var i = 0; i < 100; i++)
        {
            var token = CreateActiveToken($"user-{i}");
            tokens.Add(token);
            await _store.StoreAsync(token);
        }

        // Assert
        foreach (var token in tokens)
        {
            var retrieved = await _store.GetByTokenAsync(token.Token);
            retrieved.Should().NotBeNull();
            retrieved!.Id.Should().Be(token.Id);
        }
    }

    #endregion

    #region Capacity Limit Tests

    [Fact]
    public async Task StoreAsync_ShouldAcceptTokens_UpToMaxCapacity()
    {
        // Arrange - Fill store to capacity (10,000 tokens)
        for (var i = 0; i < 10_000; i++)
        {
            var token = CreateActiveToken($"user-{i}");
            await _store.StoreAsync(token);
        }

        // Assert - Verify last token was stored
        // If we got here without exception, all 10,000 tokens were accepted
    }

    [Fact]
    public async Task StoreAsync_ShouldThrowInvalidOperationException_WhenAtCapacityWithNoExpiredTokens()
    {
        // Arrange - Fill store to capacity with active (non-expired) tokens
        for (var i = 0; i < 10_000; i++)
        {
            var token = CreateActiveToken($"user-{i}");
            await _store.StoreAsync(token);
        }

        // Act - Try to add one more active token
        var extraToken = CreateActiveToken("overflow-user");
        var act = () => _store.StoreAsync(extraToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*maximum capacity*");
    }

    #endregion

    #region Expired Token Eviction Tests

    [Fact]
    public async Task StoreAsync_ShouldEvictExpiredTokens_WhenAtCapacity()
    {
        // Arrange - Fill store with a mix of expired and active tokens
        var expiredTokens = new List<RefreshToken>();
        for (var i = 0; i < 5_000; i++)
        {
            var expired = CreateExpiredToken($"expired-user-{i}");
            expiredTokens.Add(expired);
            await _store.StoreAsync(expired);
        }

        for (var i = 0; i < 5_000; i++)
        {
            var active = CreateActiveToken($"active-user-{i}");
            await _store.StoreAsync(active);
        }

        // Act - Adding a new token should trigger eviction of expired tokens
        var newToken = CreateActiveToken("new-user");
        var act = () => _store.StoreAsync(newToken);

        // Assert - Should not throw because expired tokens were evicted
        await act.Should().NotThrowAsync();

        // Verify the new token was stored
        var retrieved = await _store.GetByTokenAsync(newToken.Token);
        retrieved.Should().NotBeNull();

        // Verify expired tokens were evicted
        foreach (var expired in expiredTokens.Take(10)) // Check a sample
        {
            var result = await _store.GetByTokenAsync(expired.Token);
            result.Should().BeNull("expired tokens should be evicted during capacity enforcement");
        }
    }

    [Fact]
    public async Task StoreAsync_ShouldEvictRevokedTokens_WhenAtCapacity()
    {
        // Arrange - Fill store with revoked tokens and active tokens
        var revokedTokens = new List<RefreshToken>();
        for (var i = 0; i < 5_000; i++)
        {
            var token = CreateActiveToken($"revoked-user-{i}");
            token.Revoke("test revocation");
            revokedTokens.Add(token);
            await _store.StoreAsync(token);
        }

        for (var i = 0; i < 5_000; i++)
        {
            var active = CreateActiveToken($"active-user-{i}");
            await _store.StoreAsync(active);
        }

        // Act - Adding a new token should trigger eviction of revoked tokens
        var newToken = CreateActiveToken("new-user");
        var act = () => _store.StoreAsync(newToken);

        // Assert - Should not throw because revoked tokens were evicted
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Retrieval After Eviction Tests

    [Fact]
    public async Task GetByTokenAsync_ShouldReturnNull_ForEvictedExpiredToken()
    {
        // Arrange
        var expiredToken = CreateExpiredToken();
        await _store.StoreAsync(expiredToken);

        // Fill to capacity with active tokens
        for (var i = 0; i < 9_999; i++)
        {
            await _store.StoreAsync(CreateActiveToken($"filler-{i}"));
        }

        // Trigger eviction by adding one more
        await _store.StoreAsync(CreateActiveToken("trigger-user"));

        // Act
        var result = await _store.GetByTokenAsync(expiredToken.Token);

        // Assert
        result.Should().BeNull("the expired token should have been evicted");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_ForEvictedExpiredToken()
    {
        // Arrange
        var expiredToken = CreateExpiredToken();
        await _store.StoreAsync(expiredToken);

        // Fill to capacity with active tokens
        for (var i = 0; i < 9_999; i++)
        {
            await _store.StoreAsync(CreateActiveToken($"filler-{i}"));
        }

        // Trigger eviction by adding one more
        await _store.StoreAsync(CreateActiveToken("trigger-user"));

        // Act
        var result = await _store.GetByIdAsync(expiredToken.Id);

        // Assert
        result.Should().BeNull("the expired token should have been evicted");
    }

    #endregion

    public void Dispose()
    {
        _store.Dispose();
    }
}
