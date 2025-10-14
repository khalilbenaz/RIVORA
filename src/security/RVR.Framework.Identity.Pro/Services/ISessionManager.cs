using RVR.Framework.Identity.Pro.Models;

namespace RVR.Framework.Identity.Pro.Services;

/// <summary>
/// Manages user sessions including creation, retrieval, and revocation.
/// </summary>
public interface ISessionManager
{
    /// <summary>
    /// Creates a new session for the specified user.
    /// </summary>
    Task<UserSession> CreateSessionAsync(string userId, string? ipAddress = null, string? userAgent = null, CancellationToken ct = default);

    /// <summary>
    /// Gets all active (non-revoked) sessions for a user.
    /// </summary>
    Task<IReadOnlyList<UserSession>> GetActiveSessionsAsync(string userId, CancellationToken ct = default);

    /// <summary>
    /// Revokes a specific session by its identifier.
    /// </summary>
    Task<bool> RevokeSessionAsync(Guid sessionId, CancellationToken ct = default);

    /// <summary>
    /// Revokes all sessions for the specified user.
    /// </summary>
    Task<int> RevokeAllSessionsAsync(string userId, CancellationToken ct = default);

    /// <summary>
    /// Updates the last activity timestamp for a session.
    /// </summary>
    Task<bool> TouchSessionAsync(Guid sessionId, CancellationToken ct = default);
}
