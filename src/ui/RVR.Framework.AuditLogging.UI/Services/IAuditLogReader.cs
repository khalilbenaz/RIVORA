using RVR.Framework.AuditLogging.UI.Models;

namespace RVR.Framework.AuditLogging.UI.Services;

/// <summary>
/// Service for reading audit log entries with filtering and export support.
/// </summary>
public interface IAuditLogReader
{
    /// <summary>
    /// Gets recent audit log entries.
    /// </summary>
    /// <param name="count">Maximum number of entries to return.</param>
    /// <returns>A collection of audit entry view models.</returns>
    Task<IEnumerable<AuditEntryViewModel>> GetRecentLogsAsync(int count = 50);

    /// <summary>
    /// Gets audit log entries matching the specified filter criteria.
    /// </summary>
    /// <param name="filter">The filter criteria to apply.</param>
    /// <param name="maxResults">Maximum number of results to return.</param>
    /// <returns>A collection of matching audit entry view models.</returns>
    Task<IEnumerable<AuditEntryViewModel>> GetFilteredLogsAsync(AuditLogFilter filter, int maxResults = 200);

    /// <summary>
    /// Gets the distinct entity types that appear in audit logs, for populating filter dropdowns.
    /// </summary>
    Task<IEnumerable<string>> GetDistinctEntityTypesAsync();

    /// <summary>
    /// Gets the distinct action types that appear in audit logs, for populating filter dropdowns.
    /// </summary>
    Task<IEnumerable<string>> GetDistinctActionsAsync();
}
