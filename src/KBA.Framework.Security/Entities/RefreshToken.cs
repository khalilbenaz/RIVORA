namespace KBA.Framework.Security.Entities;

using System;

/// <summary>
/// Represents a refresh token used for token rotation and session management.
/// </summary>
public class RefreshToken
{
    /// <summary>
    /// Gets or sets the unique identifier for the refresh token.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the user identifier associated with this refresh token.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tenant identifier for multi-tenancy support.
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the actual refresh token value (hashed).
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expiration date and time of the refresh token.
    /// </summary>
    public DateTime ExpiresUtc { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the token was created.
    /// </summary>
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the token was last used.
    /// </summary>
    public DateTime? LastUsedUtc { get; set; }

    /// <summary>
    /// Gets or sets the IP address from which the token was created.
    /// </summary>
    public string? CreatedByIp { get; set; }

    /// <summary>
    /// Gets or sets the IP address from which the token was last used.
    /// </summary>
    public string? LastUsedByIp { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the parent refresh token (for token rotation).
    /// </summary>
    public Guid? ReplacedByTokenId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the refresh token that this token replaces.
    /// </summary>
    public Guid? ReplacesTokenId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the token has been revoked.
    /// </summary>
    public bool IsRevoked { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the token was revoked.
    /// </summary>
    public DateTime? RevokedUtc { get; set; }

    /// <summary>
    /// Gets or sets the reason for revocation.
    /// </summary>
    public string? RevokedReason { get; set; }

    /// <summary>
    /// Gets or sets the device identifier associated with this token.
    /// </summary>
    public string? DeviceId { get; set; }

    /// <summary>
    /// Gets or sets the user agent string from which the token was created.
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Gets a value indicating whether the token has expired.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > ExpiresUtc;

    /// <summary>
    /// Gets a value indicating whether the token is active (not expired and not revoked).
    /// </summary>
    public bool IsActive => !IsRevoked && !IsExpired;

    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshToken"/> class.
    /// </summary>
    public RefreshToken()
    {
    }

    /// <summary>
    /// Creates a new refresh token with the specified parameters.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="expiresUtc">The expiration date and time.</param>
    /// <param name="ipAddress">The IP address.</param>
    /// <param name="tenantId">The optional tenant identifier.</param>
    /// <param name="deviceId">The optional device identifier.</param>
    /// <param name="userAgent">The optional user agent string.</param>
    /// <returns>A new instance of <see cref="RefreshToken"/>.</returns>
    public static RefreshToken Create(
        string userId,
        DateTime expiresUtc,
        string ipAddress,
        string? tenantId = null,
        string? deviceId = null,
        string? userAgent = null)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TenantId = tenantId,
            ExpiresUtc = expiresUtc,
            CreatedUtc = DateTime.UtcNow,
            CreatedByIp = ipAddress,
            DeviceId = deviceId,
            UserAgent = userAgent
        };
    }

    /// <summary>
    /// Marks the token as revoked.
    /// </summary>
    /// <param name="reason">The reason for revocation.</param>
    /// <param name="ipAddress">The IP address performing the revocation.</param>
    public void Revoke(string reason, string? ipAddress = null)
    {
        IsRevoked = true;
        RevokedUtc = DateTime.UtcNow;
        RevokedReason = reason;
        LastUsedByIp = ipAddress;
    }

    /// <summary>
    /// Updates the last used information for the token.
    /// </summary>
    /// <param name="ipAddress">The IP address using the token.</param>
    public void MarkAsUsed(string ipAddress)
    {
        LastUsedUtc = DateTime.UtcNow;
        LastUsedByIp = ipAddress;
    }
}
