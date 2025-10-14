namespace RVR.Framework.Privacy.Models;

/// <summary>
/// Represents a Data Subject Access Request (DSAR) as required by GDPR Articles 15-20.
/// </summary>
public class DataSubjectRequest
{
    /// <summary>
    /// Gets or sets the unique identifier of the request.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the type of the data subject request.
    /// </summary>
    public DataSubjectRequestType Type { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the data subject making the request.
    /// </summary>
    public string SubjectId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current status of the request.
    /// </summary>
    public DataSubjectRequestStatus Status { get; set; } = DataSubjectRequestStatus.Pending;

    /// <summary>
    /// Gets or sets the date and time when the request was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the request was completed.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets optional notes or additional details about the request.
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// The type of data subject request as defined by GDPR.
/// </summary>
public enum DataSubjectRequestType
{
    /// <summary>Right of access (Article 15).</summary>
    Access,

    /// <summary>Right to rectification (Article 16).</summary>
    Rectification,

    /// <summary>Right to erasure / right to be forgotten (Article 17).</summary>
    Erasure,

    /// <summary>Right to data portability (Article 20).</summary>
    Portability
}

/// <summary>
/// The status of a data subject request.
/// </summary>
public enum DataSubjectRequestStatus
{
    /// <summary>Request has been received but not yet processed.</summary>
    Pending,

    /// <summary>Request is currently being processed.</summary>
    InProgress,

    /// <summary>Request has been completed successfully.</summary>
    Completed,

    /// <summary>Request was rejected (with justification).</summary>
    Rejected
}
