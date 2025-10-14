namespace RVR.Framework.Privacy.Models;

/// <summary>
/// Defines a data retention policy for a specific entity type,
/// supporting GDPR Article 5(1)(e) storage limitation principle.
/// </summary>
public class DataRetentionPolicy
{
    /// <summary>
    /// Gets or sets the type name of the entity this policy applies to.
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of days data should be retained before the retention action is taken.
    /// </summary>
    public int RetentionDays { get; set; }

    /// <summary>
    /// Gets or sets the action to take when the retention period expires.
    /// </summary>
    public RetentionAction Action { get; set; } = RetentionAction.Delete;

    /// <summary>
    /// Gets or sets an optional description of the retention policy rationale.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// The action to take when a data retention period expires.
/// </summary>
public enum RetentionAction
{
    /// <summary>Permanently delete the data.</summary>
    Delete,

    /// <summary>Anonymize the data so it can no longer be attributed to a data subject.</summary>
    Anonymize
}
