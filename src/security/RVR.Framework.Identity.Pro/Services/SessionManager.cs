using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using RVR.Framework.Identity.Pro.Models;

namespace RVR.Framework.Identity.Pro.Services;

/// <summary>
/// In-memory implementation of <see cref="ISessionManager"/>.
/// Replace with a persistent store (Redis, database) for production use.
/// </summary>
public sealed class SessionManager : ISessionManager
{
    private readonly ConcurrentDictionary<Guid, UserSession> _sessions = new();
    private readonly ILogger<SessionManager> _logger;

    public SessionManager(ILogger<SessionManager> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<UserSession> CreateSessionAsync(
        string userId,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var session = new UserSession
        {
            UserId = userId,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        _sessions[session.Id] = session;
        _logger.LogInformation("Session {SessionId} created for user {UserId}", session.Id, userId);

        return Task.FromResult(session);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<UserSession>> GetActiveSessionsAsync(string userId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var activeSessions = _sessions.Values
            .Where(s => s.UserId == userId && !s.IsRevoked)
            .OrderByDescending(s => s.LastActivityAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<UserSession>>(activeSessions);
    }

    /// <inheritdoc />
    public Task<bool> RevokeSessionAsync(Guid sessionId, CancellationToken ct = default)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            _logger.LogWarning("Cannot revoke session {SessionId}: not found", sessionId);
            return Task.FromResult(false);
        }

        if (session.IsRevoked)
        {
            _logger.LogWarning("Session {SessionId} is already revoked", sessionId);
            return Task.FromResult(false);
        }

        session.IsRevoked = true;
        _logger.LogInformation("Session {SessionId} revoked for user {UserId}", sessionId, session.UserId);
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<int> RevokeAllSessionsAsync(string userId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var count = 0;
        foreach (var session in _sessions.Values.Where(s => s.UserId == userId && !s.IsRevoked))
        {
            session.IsRevoked = true;
            count++;
        }

        _logger.LogInformation("Revoked {Count} sessions for user {UserId}", count, userId);
        return Task.FromResult(count);
    }

    /// <inheritdoc />
    public Task<bool> TouchSessionAsync(Guid sessionId, CancellationToken ct = default)
    {
        if (!_sessions.TryGetValue(sessionId, out var session) || session.IsRevoked)
            return Task.FromResult(false);

        session.LastActivityAt = DateTime.UtcNow;
        return Task.FromResult(true);
    }
}
