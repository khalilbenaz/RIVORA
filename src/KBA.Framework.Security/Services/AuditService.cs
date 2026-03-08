namespace KBA.Framework.Security.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KBA.Framework.Security.Entities;
using KBA.Framework.Security.Interfaces;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for managing audit trail entries.
/// </summary>
public class AuditService : IAuditService
{
    private readonly IAuditStore _store;
    private readonly ILogger<AuditService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditService"/> class.
    /// </summary>
    /// <param name="store">The audit store.</param>
    /// <param name="logger">The logger.</param>
    public AuditService(IAuditStore store, ILogger<AuditService> logger)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task LogAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        if (entry == null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        await _store.StoreAsync(entry, cancellationToken);

        _logger.LogInformation(
            "Audit entry logged: {ActionType} on {EntityType} ({EntityKey}) by user {UserId}",
            entry.ActionType, entry.EntityType, entry.EntityKey, entry.UserId);
    }

    /// <inheritdoc/>
    public Task<IEnumerable<AuditEntry>> GetByEntityAsync(
        string entityType,
        string entityKey,
        CancellationToken cancellationToken = default)
    {
        return _store.GetByEntityAsync(entityType, entityKey, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IEnumerable<AuditEntry>> GetByUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return _store.GetByUserAsync(userId, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IEnumerable<AuditEntry>> GetByTimeRangeAsync(
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken = default)
    {
        return _store.GetByTimeRangeAsync(startTime, endTime, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IEnumerable<AuditEntry>> GetByActionTypeAsync(
        AuditActionType actionType,
        CancellationToken cancellationToken = default)
    {
        return _store.GetByActionTypeAsync(actionType, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IEnumerable<AuditEntry>> GetByTenantAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        return _store.GetByTenantAsync(tenantId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> CleanupAsync(DateTime olderThan, CancellationToken cancellationToken = default)
    {
        var deletedCount = await _store.DeleteOlderThanAsync(olderThan, cancellationToken);

        _logger.LogInformation(
            "Audit cleanup completed: deleted {Count} entries older than {Date}",
            deletedCount, olderThan);

        return deletedCount;
    }
}
