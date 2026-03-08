namespace KBA.Framework.Security.Interfaces;

using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Defines the contract for audit store operations.
/// </summary>
public interface IAuditStore
{
    /// <summary>
    /// Stores an audit entry.
    /// </summary>
    /// <param name="entry">The audit entry to store.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StoreAsync(Entities.AuditEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves audit entries by entity.
    /// </summary>
    Task<IEnumerable<Entities.AuditEntry>> GetByEntityAsync(
        string entityType,
        string entityKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves audit entries by user.
    /// </summary>
    Task<IEnumerable<Entities.AuditEntry>> GetByUserAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves audit entries by time range.
    /// </summary>
    Task<IEnumerable<Entities.AuditEntry>> GetByTimeRangeAsync(
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves audit entries by action type.
    /// </summary>
    Task<IEnumerable<Entities.AuditEntry>> GetByActionTypeAsync(
        Entities.AuditActionType actionType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves audit entries by tenant.
    /// </summary>
    Task<IEnumerable<Entities.AuditEntry>> GetByTenantAsync(
        string tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes audit entries older than the specified date.
    /// </summary>
    Task<int> DeleteOlderThanAsync(DateTime olderThan, CancellationToken cancellationToken = default);
}
