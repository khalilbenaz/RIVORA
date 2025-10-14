namespace RVR.Framework.Security.Interfaces;

using System;
using System.Threading;
using System.Threading.Tasks;
using RVR.Framework.Security.Entities;

/// <summary>
/// Defines the contract for audit service operations.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Logs an audit entry.
    /// </summary>
    /// <param name="entry">The audit entry to log.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task LogAsync(AuditEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves audit entries for a specific entity.
    /// </summary>
    /// <param name="entityType">Type of the entity.</param>
    /// <param name="entityKey">The entity key.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of audit entries.</returns>
    Task<IEnumerable<AuditEntry>> GetByEntityAsync(
        string entityType,
        string entityKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves audit entries for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of audit entries.</returns>
    Task<IEnumerable<AuditEntry>> GetByUserAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves audit entries within a time range.
    /// </summary>
    /// <param name="startTime">The start time.</param>
    /// <param name="endTime">The end time.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of audit entries.</returns>
    Task<IEnumerable<AuditEntry>> GetByTimeRangeAsync(
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves audit entries for a specific action type.
    /// </summary>
    /// <param name="actionType">Type of the action.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of audit entries.</returns>
    Task<IEnumerable<AuditEntry>> GetByActionTypeAsync(
        AuditActionType actionType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves audit entries for a specific tenant.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of audit entries.</returns>
    Task<IEnumerable<AuditEntry>> GetByTenantAsync(
        string tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cleans up old audit entries.
    /// </summary>
    /// <param name="olderThan">Delete entries older than this date.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of entries deleted.</returns>
    Task<int> CleanupAsync(DateTime olderThan, CancellationToken cancellationToken = default);
}
