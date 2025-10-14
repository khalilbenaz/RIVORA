using RVR.Framework.Infrastructure.Data;
using RVR.Framework.Security.Entities;
using RVR.Framework.Security.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace RVR.Framework.Infrastructure.Services;

/// <summary>
/// EF Core implementation of <see cref="IRefreshTokenStore"/> that persists refresh tokens
/// using the domain RefreshToken entity via RVRDbContext.
/// </summary>
public class EfRefreshTokenStore : IRefreshTokenStore
{
    private readonly RVRDbContext _context;

    public EfRefreshTokenStore(RVRDbContext context)
    {
        _context = context;
    }

    public async Task StoreAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        var domainToken = ToDomain(token);
        var existing = await _context.RefreshTokens.FindAsync([domainToken.Id], cancellationToken);

        if (existing != null)
        {
            _context.Entry(existing).CurrentValues.SetValues(domainToken);
        }
        else
        {
            await _context.RefreshTokens.AddAsync(domainToken, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var domainToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

        return domainToken != null ? ToSecurity(domainToken) : null;
    }

    public async Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var domainToken = await _context.RefreshTokens.FindAsync([id], cancellationToken);
        return domainToken != null ? ToSecurity(domainToken) : null;
    }

    public async Task<IEnumerable<RefreshToken>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(userId, out var userGuid))
            return [];

        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userGuid)
            .OrderByDescending(rt => rt.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return tokens.Select(ToSecurity);
    }

    public async Task RevokeAsync(RefreshToken token, string reason, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var domainToken = await _context.RefreshTokens.FindAsync([token.Id], cancellationToken);
        if (domainToken != null)
        {
            domainToken.RevokedAtUtc = DateTime.UtcNow;
            domainToken.RevokedByIp = ipAddress;
            domainToken.RevokedReason = reason;
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Update the in-memory security token as well
        token.IsRevoked = true;
        token.RevokedUtc = DateTime.UtcNow;
        token.RevokedReason = reason;
    }

    public async Task RevokeAllByUserIdAsync(string userId, string reason, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(userId, out var userGuid))
            return;

        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userGuid && rt.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.RevokedAtUtc = DateTime.UtcNow;
            token.RevokedReason = reason;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> RemoveExpiredAsync(CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-30);
        var expired = await _context.RefreshTokens
            .Where(rt => rt.ExpiresAtUtc < cutoff)
            .ToListAsync(cancellationToken);

        _context.RefreshTokens.RemoveRange(expired);
        await _context.SaveChangesAsync(cancellationToken);
        return expired.Count;
    }

    public async Task<int> RemoveRevokedAsync(CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-30);
        var revoked = await _context.RefreshTokens
            .Where(rt => rt.RevokedAtUtc != null && rt.RevokedAtUtc < cutoff)
            .ToListAsync(cancellationToken);

        _context.RefreshTokens.RemoveRange(revoked);
        await _context.SaveChangesAsync(cancellationToken);
        return revoked.Count;
    }

    /// <summary>
    /// Maps a Security layer RefreshToken to the Domain entity.
    /// </summary>
    private static Domain.Entities.Security.RefreshToken ToDomain(RefreshToken securityToken)
    {
        Guid.TryParse(securityToken.UserId, out var userId);
        Guid? tenantId = Guid.TryParse(securityToken.TenantId, out var tid) ? tid : null;

        return new Domain.Entities.Security.RefreshToken
        {
            Id = securityToken.Id,
            UserId = userId,
            Token = securityToken.Token,
            ExpiresAtUtc = securityToken.ExpiresUtc,
            CreatedAtUtc = securityToken.CreatedUtc,
            CreatedByIp = securityToken.CreatedByIp,
            RevokedAtUtc = securityToken.RevokedUtc,
            RevokedByIp = securityToken.LastUsedByIp,
            ReplacedByToken = securityToken.ReplacedByTokenId?.ToString(),
            RevokedReason = securityToken.RevokedReason,
            DeviceId = securityToken.DeviceId,
            UserAgent = securityToken.UserAgent,
            TenantId = tenantId
        };
    }

    /// <summary>
    /// Maps a Domain entity to the Security layer RefreshToken.
    /// </summary>
    private static RefreshToken ToSecurity(Domain.Entities.Security.RefreshToken domainToken)
    {
        return new RefreshToken
        {
            Id = domainToken.Id,
            UserId = domainToken.UserId.ToString(),
            Token = domainToken.Token,
            ExpiresUtc = domainToken.ExpiresAtUtc,
            CreatedUtc = domainToken.CreatedAtUtc,
            CreatedByIp = domainToken.CreatedByIp,
            IsRevoked = domainToken.RevokedAtUtc != null,
            RevokedUtc = domainToken.RevokedAtUtc,
            RevokedReason = domainToken.RevokedReason,
            ReplacedByTokenId = Guid.TryParse(domainToken.ReplacedByToken, out var replacedId) ? replacedId : null,
            DeviceId = domainToken.DeviceId,
            UserAgent = domainToken.UserAgent,
            TenantId = domainToken.TenantId?.ToString(),
            LastUsedByIp = domainToken.RevokedByIp
        };
    }
}
