namespace RVR.Framework.Identity.Pro.Models;

/// <summary>
/// Represents an active user session with tracking metadata.
/// </summary>
public sealed class UserSession
{
    /// <summary>
    /// Gets or sets the unique session identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the user who owns this session.
    /// </summary>
    public required string UserId { get; set; }

    /// <summary>
    /// Gets or sets the IP address from which the session was created.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Gets or sets the user agent string of the client.
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the session was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time of the last activity in this session.
    /// </summary>
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets whether this session has been revoked.
    /// </summary>
    public bool IsRevoked { get; set; }
}
