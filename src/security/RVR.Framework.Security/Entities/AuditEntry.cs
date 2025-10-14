namespace RVR.Framework.Security.Entities;

using System;

/// <summary>
/// Represents an audit trail entry for tracking data changes.
/// </summary>
public class AuditEntry
{
    /// <summary>
    /// Gets or sets the unique identifier for the audit entry.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the user identifier who performed the action.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Gets or sets the tenant identifier for multi-tenancy support.
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the type of entity being audited.
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the primary key of the affected entity.
    /// </summary>
    public string? EntityKey { get; set; }

    /// <summary>
    /// Gets or sets the type of audit action (Create, Update, Delete).
    /// </summary>
    public AuditActionType ActionType { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the action occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the old values (JSON format) for update and delete operations.
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// Gets or sets the new values (JSON format) for create and update operations.
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    /// Gets or sets the affected properties (comma-separated list).
    /// </summary>
    public string? AffectedColumns { get; set; }

    /// <summary>
    /// Gets or sets the IP address from which the action was performed.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Gets or sets the user agent string.
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Gets or sets the correlation ID for distributed tracing.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets additional metadata (JSON format).
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditEntry"/> class.
    /// </summary>
    public AuditEntry()
    {
    }

    /// <summary>
    /// Creates a new audit entry for a create operation.
    /// </summary>
    /// <param name="entityType">Type of the entity.</param>
    /// <param name="entityKey">The entity key.</param>
    /// <param name="newValues">The new values.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="ipAddress">The IP address.</param>
    /// <returns>A new <see cref="AuditEntry"/> instance.</returns>
    public static AuditEntry CreateCreate(
        string entityType,
        string entityKey,
        string newValues,
        string? userId = null,
        string? tenantId = null,
        string? ipAddress = null)
    {
        return new AuditEntry
        {
            EntityType = entityType,
            EntityKey = entityKey,
            ActionType = AuditActionType.Create,
            NewValues = newValues,
            UserId = userId,
            TenantId = tenantId,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a new audit entry for an update operation.
    /// </summary>
    /// <param name="entityType">Type of the entity.</param>
    /// <param name="entityKey">The entity key.</param>
    /// <param name="oldValues">The old values.</param>
    /// <param name="newValues">The new values.</param>
    /// <param name="affectedColumns">The affected columns.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="ipAddress">The IP address.</param>
    /// <returns>A new <see cref="AuditEntry"/> instance.</returns>
    public static AuditEntry CreateUpdate(
        string entityType,
        string entityKey,
        string oldValues,
        string newValues,
        string affectedColumns,
        string? userId = null,
        string? tenantId = null,
        string? ipAddress = null)
    {
        return new AuditEntry
        {
            EntityType = entityType,
            EntityKey = entityKey,
            ActionType = AuditActionType.Update,
            OldValues = oldValues,
            NewValues = newValues,
            AffectedColumns = affectedColumns,
            UserId = userId,
            TenantId = tenantId,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a new audit entry for a delete operation.
    /// </summary>
    /// <param name="entityType">Type of the entity.</param>
    /// <param name="entityKey">The entity key.</param>
    /// <param name="oldValues">The old values.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="ipAddress">The IP address.</param>
    /// <returns>A new <see cref="AuditEntry"/> instance.</returns>
    public static AuditEntry CreateDelete(
        string entityType,
        string entityKey,
        string oldValues,
        string? userId = null,
        string? tenantId = null,
        string? ipAddress = null)
    {
        return new AuditEntry
        {
            EntityType = entityType,
            EntityKey = entityKey,
            ActionType = AuditActionType.Delete,
            OldValues = oldValues,
            UserId = userId,
            TenantId = tenantId,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Represents the type of audit action.
/// </summary>
public enum AuditActionType
{
    /// <summary>
    /// Entity was created.
    /// </summary>
    Create = 0,

    /// <summary>
    /// Entity was updated.
    /// </summary>
    Update = 1,

    /// <summary>
    /// Entity was deleted.
    /// </summary>
    Delete = 2
}
