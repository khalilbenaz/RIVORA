namespace RVR.Framework.Security.Services;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RVR.Framework.Security.Entities;
using RVR.Framework.Security.Interfaces;

/// <summary>
/// In-memory implementation of <see cref="IAuditStore"/>.
/// Uses thread-safe concurrent collections for storage.
/// </summary>
public class InMemoryAuditStore : IAuditStore
{
    private readonly ConcurrentBag<AuditEntry> _entries = new();
    private readonly ReaderWriterLockSlim _lock = new();

    /// <inheritdoc/>
    public Task StoreAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        if (entry == null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        _entries.Add(entry);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<IEnumerable<AuditEntry>> GetByEntityAsync(
        string entityType,
        string entityKey,
        CancellationToken cancellationToken = default)
    {
        _lock.EnterReadLock();
        try
        {
            var entries = _entries.Where(e =>
                e.EntityType == entityType &&
                (string.IsNullOrEmpty(entityKey) || e.EntityKey == entityKey))
                .OrderByDescending(e => e.Timestamp)
                .ToList();

            return Task.FromResult<IEnumerable<AuditEntry>>(entries);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <inheritdoc/>
    public Task<IEnumerable<AuditEntry>> GetByUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        _lock.EnterReadLock();
        try
        {
            var entries = _entries.Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Timestamp)
                .ToList();

            return Task.FromResult<IEnumerable<AuditEntry>>(entries);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <inheritdoc/>
    public Task<IEnumerable<AuditEntry>> GetByTimeRangeAsync(
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken = default)
    {
        _lock.EnterReadLock();
        try
        {
            var entries = _entries.Where(e =>
                e.Timestamp >= startTime && e.Timestamp <= endTime)
                .OrderBy(e => e.Timestamp)
                .ToList();

            return Task.FromResult<IEnumerable<AuditEntry>>(entries);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <inheritdoc/>
    public Task<IEnumerable<AuditEntry>> GetByActionTypeAsync(
        AuditActionType actionType,
        CancellationToken cancellationToken = default)
    {
        _lock.EnterReadLock();
        try
        {
            var entries = _entries.Where(e => e.ActionType == actionType)
                .OrderByDescending(e => e.Timestamp)
                .ToList();

            return Task.FromResult<IEnumerable<AuditEntry>>(entries);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <inheritdoc/>
    public Task<IEnumerable<AuditEntry>> GetByTenantAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        _lock.EnterReadLock();
        try
        {
            var entries = _entries.Where(e => e.TenantId == tenantId)
                .OrderByDescending(e => e.Timestamp)
                .ToList();

            return Task.FromResult<IEnumerable<AuditEntry>>(entries);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <inheritdoc/>
    public Task<int> DeleteOlderThanAsync(DateTime olderThan, CancellationToken cancellationToken = default)
    {
        _lock.EnterWriteLock();
        try
        {
            var entriesToDelete = _entries.Where(e => e.Timestamp < olderThan).ToList();

            foreach (var entry in entriesToDelete)
            {
                _entries.TryTake(out _);
            }

            return Task.FromResult(entriesToDelete.Count);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}
