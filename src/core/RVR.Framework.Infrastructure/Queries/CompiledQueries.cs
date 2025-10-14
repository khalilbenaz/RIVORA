using RVR.Framework.Domain.Entities.Security;
using RVR.Framework.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace RVR.Framework.Infrastructure.Queries;

/// <summary>
/// Centralized compiled EF Core queries for optimal performance.
/// Compiled queries avoid the overhead of query expression tree translation on each execution.
/// <para>
/// Only framework-level queries belong here. Business-specific compiled queries
/// (e.g., for Product or User entities) should be declared as private static fields
/// in their respective repository classes.
/// </para>
/// </summary>
public static class CompiledQueries
{
    #region RefreshToken Queries

    /// <summary>
    /// Gets a refresh token by its token string (with tracking for revocation scenarios).
    /// </summary>
    public static readonly Func<RVRDbContext, string, Task<RefreshToken?>> GetRefreshTokenByToken =
        EF.CompileAsyncQuery((RVRDbContext ctx, string token) =>
            ctx.RefreshTokens.FirstOrDefault(rt => rt.Token == token));

    /// <summary>
    /// Gets all refresh tokens for a user ordered by creation date descending (with tracking).
    /// </summary>
    public static readonly Func<RVRDbContext, Guid, IAsyncEnumerable<RefreshToken>> GetRefreshTokensByUserId =
        EF.CompileAsyncQuery((RVRDbContext ctx, Guid userId) =>
            ctx.RefreshTokens
                .Where(rt => rt.UserId == userId)
                .OrderByDescending(rt => rt.CreatedAtUtc)
                .AsQueryable());

    /// <summary>
    /// Gets active (non-revoked) refresh tokens for a user (with tracking for revocation).
    /// </summary>
    public static readonly Func<RVRDbContext, Guid, IAsyncEnumerable<RefreshToken>> GetActiveRefreshTokensByUserId =
        EF.CompileAsyncQuery((RVRDbContext ctx, Guid userId) =>
            ctx.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.RevokedAtUtc == null)
                .AsQueryable());

    #endregion
}
