namespace RVR.Framework.Security.Services;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RVR.Framework.Security.Interfaces;

/// <summary>
/// In-memory implementation of <see cref="IBackupCodeStore"/>.
/// Uses thread-safe concurrent collections for storage.
/// </summary>
public class InMemoryBackupCodeStore : IBackupCodeStore
{
    /// <summary>
    /// Stores hashed backup codes per user.
    /// Key: UserId, Value: Set of hashed codes
    /// </summary>
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, BackupCodeEntry>> _userCodes
        = new();

    /// <inheritdoc/>
    public Task StoreAsync(string userId, IEnumerable<string> hashedCodes, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        }

        var codes = new ConcurrentDictionary<string, BackupCodeEntry>();
        var now = DateTime.UtcNow;

        foreach (var hashedCode in hashedCodes)
        {
            codes[hashedCode] = new BackupCodeEntry
            {
                HashedCode = hashedCode,
                CreatedAt = now,
                IsRevoked = false
            };
        }

        _userCodes.AddOrUpdate(userId, codes, (_, _) => codes);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<bool> ValidateAsync(string userId, string hashedCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(hashedCode))
        {
            return Task.FromResult(false);
        }

        if (!_userCodes.TryGetValue(userId, out var codes))
        {
            return Task.FromResult(false);
        }

        if (!codes.TryGetValue(hashedCode, out var entry))
        {
            return Task.FromResult(false);
        }

        // Check if code is already revoked
        if (entry.IsRevoked)
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    /// <inheritdoc/>
    public Task RevokeAsync(string userId, string hashedCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(hashedCode))
        {
            return Task.CompletedTask;
        }

        if (_userCodes.TryGetValue(userId, out var codes))
        {
            if (codes.TryGetValue(hashedCode, out var entry))
            {
                entry.IsRevoked = true;
                entry.RevokedAt = DateTime.UtcNow;
            }
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<int> GetRemainingCountAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Task.FromResult(0);
        }

        if (!_userCodes.TryGetValue(userId, out var codes))
        {
            return Task.FromResult(0);
        }

        var remainingCount = codes.Values.Count(c => !c.IsRevoked);
        return Task.FromResult(remainingCount);
    }

    private class BackupCodeEntry
    {
        public string HashedCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }
    }
}
