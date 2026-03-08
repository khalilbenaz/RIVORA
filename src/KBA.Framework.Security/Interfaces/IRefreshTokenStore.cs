namespace KBA.Framework.Security.Interfaces;

using System;
using System.Threading;
using System.Threading.Tasks;
using KBA.Framework.Security.Entities;

/// <summary>
/// Defines the contract for refresh token storage operations.
/// </summary>
public interface IRefreshTokenStore
{
    /// <summary>
    /// Stores a new refresh token.
    /// </summary>
    /// <param name="token">The refresh token to store.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StoreAsync(RefreshToken token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a refresh token by its token value.
    /// </summary>
    /// <param name="token">The token value to search for.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The refresh token if found; otherwise, null.</returns>
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a refresh token by its identifier.
    /// </summary>
    /// <param name="id">The token identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The refresh token if found; otherwise, null.</returns>
    Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all active refresh tokens for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of active refresh tokens.</returns>
    Task<IEnumerable<RefreshToken>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a refresh token.
    /// </summary>
    /// <param name="token">The token to revoke.</param>
    /// <param name="reason">The reason for revocation.</param>
    /// <param name="ipAddress">The IP address performing the revocation.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RevokeAsync(RefreshToken token, string reason, string? ipAddress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes all refresh tokens for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="reason">The reason for revocation.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RevokeAllByUserIdAsync(string userId, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes expired tokens from the store.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of tokens removed.</returns>
    Task<int> RemoveExpiredAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes revoked tokens from the store.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of tokens removed.</returns>
    Task<int> RemoveRevokedAsync(CancellationToken cancellationToken = default);
}
