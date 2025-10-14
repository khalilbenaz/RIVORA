// ---------------------------------------------------------------------------
// EXAMPLE IMPLEMENTATION - NOT CORE FRAMEWORK CODE
// ---------------------------------------------------------------------------
// This repository is a sample/reference implementation provided as part of
// the Rivora Framework starter template. It is business-specific and should
// be overridden or replaced in your own application's Infrastructure layer.
//
// The generic Repository<TEntity, TKey> base class (see Repository.cs) is
// the actual framework code and is intended to be reused as-is.
// ---------------------------------------------------------------------------

using RVR.Framework.Domain.Entities.Security;
using RVR.Framework.Domain.Repositories;
using RVR.Framework.Infrastructure.Data;
using RVR.Framework.Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;

namespace RVR.Framework.Infrastructure.Repositories;

/// <summary>
/// <b>Example implementation</b> – EF Core implementation of <see cref="IRefreshTokenRepository"/>.
/// <para>
/// This class is provided as a reference/starter implementation and is <b>not</b>
/// part of the core Rivora Framework. Consumers of the framework should create
/// their own refresh-token repository in their application's Infrastructure project,
/// implementing <see cref="IRefreshTokenRepository"/>.
/// </para>
/// </summary>
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly RVRDbContext _context;

    public RefreshTokenRepository(RVRDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
    {
        return await CompiledQueries.GetRefreshTokenByToken(_context, token);
    }

    public async Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.RefreshTokens.FindAsync([id], ct);
    }

    public async Task<IReadOnlyList<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var result = new List<RefreshToken>();
        await foreach (var token in CompiledQueries.GetRefreshTokensByUserId(_context, userId).WithCancellation(ct))
        {
            result.Add(token);
        }
        return result;
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync(ct);
    }

    public async Task RevokeAllForUserAsync(Guid userId, string reason, string? revokedByIp = null, CancellationToken ct = default)
    {
        var activeTokens = new List<RefreshToken>();
        await foreach (var token in CompiledQueries.GetActiveRefreshTokensByUserId(_context, userId).WithCancellation(ct))
        {
            activeTokens.Add(token);
        }

        foreach (var token in activeTokens)
        {
            token.RevokedAtUtc = DateTime.UtcNow;
            token.RevokedByIp = revokedByIp;
            token.RevokedReason = reason;
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task CleanupExpiredAsync(CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-30);

        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.ExpiresAtUtc < cutoff || (rt.RevokedAtUtc != null && rt.RevokedAtUtc < cutoff))
            .ToListAsync(ct);

        _context.RefreshTokens.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync(ct);
    }
}
