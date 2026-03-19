namespace RVR.Framework.Security.Services;

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RVR.Framework.Security.Entities;
using RVR.Framework.Security.Interfaces;

/// <summary>
/// In-memory implementation of <see cref="IRefreshTokenStore"/>.
/// Uses thread-safe concurrent collections for storage.
/// Enforces a maximum capacity to prevent unbounded memory growth.
/// </summary>
public class InMemoryRefreshTokenStore : IRefreshTokenStore, IDisposable
{
    private const int MaxCapacity = 10_000;

    private readonly ConcurrentDictionary<Guid, RefreshToken> _tokensById = new();
    private readonly ConcurrentDictionary<string, Guid> _tokensByValue = new();
    private readonly ReaderWriterLockSlim _lock = new();

    /// <inheritdoc/>
    public Task StoreAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        if (token == null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        _lock.EnterWriteLock();
        try
        {
            // Evict expired tokens when approaching capacity
            if (_tokensById.Count >= MaxCapacity)
            {
                EvictExpiredTokensUnsafe();
            }

            if (_tokensById.Count >= MaxCapacity)
            {
                throw new InvalidOperationException(
                    $"In-memory refresh token store has reached maximum capacity ({MaxCapacity}). " +
                    "Consider using a persistent store (EfRefreshTokenStore) for production.");
            }

            _tokensById[token.Id] = token;
            _tokensByValue[token.Token] = token.Id;
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        return Task.CompletedTask;
    }

    private void EvictExpiredTokensUnsafe()
    {
        var expired = _tokensById.Values
            .Where(t => t.IsExpired || t.IsRevoked)
            .ToList();

        foreach (var t in expired)
        {
            _tokensById.TryRemove(t.Id, out _);
            _tokensByValue.TryRemove(t.Token, out _);
        }
    }

    /// <inheritdoc/>
    public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult<RefreshToken?>(null);
        }

        _lock.EnterReadLock();
        try
        {
            if (_tokensByValue.TryGetValue(token, out var id))
            {
                _tokensById.TryGetValue(id, out var refreshToken);
                return Task.FromResult(refreshToken);
            }

            return Task.FromResult<RefreshToken?>(null);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <inheritdoc/>
    public Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _lock.EnterReadLock();
        try
        {
            _tokensById.TryGetValue(id, out var token);
            return Task.FromResult(token);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <inheritdoc/>
    public Task<IEnumerable<RefreshToken>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Task.FromResult(Enumerable.Empty<RefreshToken>());
        }

        _lock.EnterReadLock();
        try
        {
            var tokens = _tokensById.Values
                .Where(t => t.UserId == userId)
                .ToList();

            return Task.FromResult<IEnumerable<RefreshToken>>(tokens);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <inheritdoc/>
    public Task RevokeAsync(RefreshToken token, string reason, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        if (token == null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        _lock.EnterWriteLock();
        try
        {
            token.Revoke(reason, ipAddress);

            // Update the stored token
            if (_tokensById.ContainsKey(token.Id))
            {
                _tokensById[token.Id] = token;
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RevokeAllByUserIdAsync(string userId, string reason, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Task.CompletedTask;
        }

        _lock.EnterWriteLock();
        try
        {
            var tokensToRevoke = _tokensById.Values
                .Where(t => t.UserId == userId && !t.IsRevoked)
                .ToList();

            foreach (var token in tokensToRevoke)
            {
                token.Revoke(reason, null);
                _tokensById[token.Id] = token;
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<int> RemoveExpiredAsync(CancellationToken cancellationToken = default)
    {
        _lock.EnterWriteLock();
        try
        {
            var expiredTokens = _tokensById.Values
                .Where(t => t.IsExpired)
                .ToList();

            foreach (var token in expiredTokens)
            {
                _tokensById.TryRemove(token.Id, out _);
                _tokensByValue.TryRemove(token.Token, out _);
            }

            return Task.FromResult(expiredTokens.Count);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <inheritdoc/>
    public Task<int> RemoveRevokedAsync(CancellationToken cancellationToken = default)
    {
        _lock.EnterWriteLock();
        try
        {
            var revokedTokens = _tokensById.Values
                .Where(t => t.IsRevoked && t.RevokedUtc.HasValue &&
                           t.RevokedUtc.Value.AddDays(30) < DateTime.UtcNow)
                .ToList();

            foreach (var token in revokedTokens)
            {
                _tokensById.TryRemove(token.Id, out _);
                _tokensByValue.TryRemove(token.Token, out _);
            }

            return Task.FromResult(revokedTokens.Count);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void Dispose()
    {
        _lock.Dispose();
    }
}
