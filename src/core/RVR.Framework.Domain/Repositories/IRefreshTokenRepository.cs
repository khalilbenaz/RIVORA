using RVR.Framework.Domain.Entities.Security;

namespace RVR.Framework.Domain.Repositories;

/// <summary>
/// Repository for persistent refresh token storage.
/// </summary>
public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default);
    Task UpdateAsync(RefreshToken refreshToken, CancellationToken ct = default);
    Task RevokeAllForUserAsync(Guid userId, string reason, string? revokedByIp = null, CancellationToken ct = default);
    Task CleanupExpiredAsync(CancellationToken ct = default);
}
