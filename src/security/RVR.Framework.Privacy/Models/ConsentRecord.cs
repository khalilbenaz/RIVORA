namespace RVR.Framework.Privacy.Models;

/// <summary>
/// Represents a record of consent given by a data subject for a specific processing purpose.
/// Supports GDPR Article 7 requirements for demonstrable consent.
/// </summary>
public class ConsentRecord
{
    /// <summary>
    /// Gets or sets the unique identifier of the consent record.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the identifier of the data subject who gave consent.
    /// </summary>
    public string SubjectId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the purpose for which consent was given.
    /// </summary>
    public string Purpose { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether consent has been granted.
    /// </summary>
    public bool Granted { get; set; }

    /// <summary>
    /// Gets or sets the date and time when consent was granted.
    /// </summary>
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when consent was revoked, if applicable.
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this consent expires.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Gets whether this consent is currently active (granted, not revoked, and not expired).
    /// </summary>
    public bool IsActive =>
        Granted &&
        RevokedAt == null &&
        (ExpiresAt == null || ExpiresAt > DateTime.UtcNow);
}
