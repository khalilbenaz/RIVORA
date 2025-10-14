namespace RVR.Framework.Security.Services;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using RVR.Framework.Security.Interfaces;

/// <summary>
/// In-memory implementation of <see cref="IPermissionStore"/>.
/// Uses thread-safe concurrent collections for storage.
/// </summary>
public class InMemoryPermissionStore : IPermissionStore
{
    /// <summary>
    /// Stores permissions per user.
    /// Key: UserId, Value: Set of permissions
    /// </summary>
    private readonly ConcurrentDictionary<string, ConcurrentHashSet> _userPermissions = new();

    /// <summary>
    /// Stores all registered permissions in the system.
    /// </summary>
    private readonly ConcurrentHashSet _allPermissions = new();

    /// <inheritdoc/>
    public Task<IEnumerable<string>> GetPermissionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Task.FromResult<IEnumerable<string>>(Enumerable.Empty<string>());
        }

        if (_userPermissions.TryGetValue(userId, out var permissions))
        {
            return Task.FromResult<IEnumerable<string>>(permissions.ToArray());
        }

        return Task.FromResult<IEnumerable<string>>(Enumerable.Empty<string>());
    }

    /// <inheritdoc/>
    public Task GrantPermissionAsync(string userId, string permission, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(permission))
        {
            throw new ArgumentException("Permission cannot be empty.", nameof(permission));
        }

        _userPermissions.AddOrUpdate(
            userId,
            _ => new ConcurrentHashSet(new[] { permission }),
            (_, existing) =>
            {
                existing.Add(permission);
                return existing;
            });

        _allPermissions.Add(permission);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RevokePermissionAsync(string userId, string permission, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(permission))
        {
            throw new ArgumentException("Permission cannot be empty.", nameof(permission));
        }

        if (_userPermissions.TryGetValue(userId, out var permissions))
        {
            permissions.Remove(permission);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<IEnumerable<string>> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<string>>(_allPermissions.ToArray());
    }

    /// <summary>
    /// Thread-safe hash set implementation.
    /// </summary>
    private class ConcurrentHashSet
    {
        private readonly ConcurrentDictionary<string, byte> _dictionary = new();

        public ConcurrentHashSet()
        {
        }

        public ConcurrentHashSet(IEnumerable<string> items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        public bool Add(string item)
        {
            return _dictionary.TryAdd(item.ToLowerInvariant(), 0);
        }

        public bool Remove(string item)
        {
            return _dictionary.TryRemove(item.ToLowerInvariant(), out _);
        }

        public bool Contains(string item)
        {
            return _dictionary.ContainsKey(item.ToLowerInvariant());
        }

        public string[] ToArray()
        {
            return _dictionary.Keys.ToArray();
        }
    }
}

/// <summary>
/// Extension methods for working with permission claims.
/// </summary>
public static class PermissionClaimsPrincipalExtensions
{
    /// <summary>
    /// The claim type used for permission claims.
    /// </summary>
    public const string PermissionClaimType = "permission";

    /// <summary>
    /// Checks if the claims principal has a specific permission.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <param name="permission">The permission to check.</param>
    /// <returns>True if the principal has the permission.</returns>
    public static bool HasPermission(this ClaimsPrincipal principal, string permission)
    {
        if (principal == null)
        {
            throw new ArgumentNullException(nameof(principal));
        }

        if (string.IsNullOrWhiteSpace(permission))
        {
            return false;
        }

        return principal.HasClaim(PermissionClaimType, permission);
    }

    /// <summary>
    /// Checks if the claims principal has any of the specified permissions.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <param name="permissions">The permissions to check.</param>
    /// <returns>True if the principal has at least one of the permissions.</returns>
    public static bool HasAnyPermission(this ClaimsPrincipal principal, params string[] permissions)
    {
        if (principal == null)
        {
            throw new ArgumentNullException(nameof(principal));
        }

        if (permissions == null || permissions.Length == 0)
        {
            return false;
        }

        return principal.Claims.Any(c =>
            c.Type == PermissionClaimType &&
            permissions.Contains(c.Value, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if the claims principal has all of the specified permissions.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <param name="permissions">The permissions to check.</param>
    /// <returns>True if the principal has all of the permissions.</returns>
    public static bool HasAllPermissions(this ClaimsPrincipal principal, params string[] permissions)
    {
        if (principal == null)
        {
            throw new ArgumentNullException(nameof(principal));
        }

        if (permissions == null || permissions.Length == 0)
        {
            return true;
        }

        var principalPermissions = principal.Claims
            .Where(c => c.Type == PermissionClaimType)
            .Select(c => c.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return permissions.All(p => principalPermissions.Contains(p));
    }

    /// <summary>
    /// Gets all permissions for the claims principal.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <returns>A collection of permission strings.</returns>
    public static IEnumerable<string> GetPermissions(this ClaimsPrincipal principal)
    {
        if (principal == null)
        {
            throw new ArgumentNullException(nameof(principal));
        }

        return principal.Claims
            .Where(c => c.Type == PermissionClaimType)
            .Select(c => c.Value);
    }

    /// <summary>
    /// Adds a permission claim to the claims identity.
    /// </summary>
    /// <param name="identity">The claims identity.</param>
    /// <param name="permission">The permission to add.</param>
    public static void AddPermissionClaim(this ClaimsIdentity identity, string permission)
    {
        if (identity == null)
        {
            throw new ArgumentNullException(nameof(identity));
        }

        if (!string.IsNullOrWhiteSpace(permission))
        {
            identity.AddClaim(new Claim(PermissionClaimType, permission));
        }
    }

    /// <summary>
    /// Adds multiple permission claims to the claims identity.
    /// </summary>
    /// <param name="identity">The claims identity.</param>
    /// <param name="permissions">The permissions to add.</param>
    public static void AddPermissionClaims(this ClaimsIdentity identity, IEnumerable<string> permissions)
    {
        if (identity == null)
        {
            throw new ArgumentNullException(nameof(identity));
        }

        if (permissions != null)
        {
            foreach (var permission in permissions)
            {
                if (!string.IsNullOrWhiteSpace(permission))
                {
                    identity.AddClaim(new Claim(PermissionClaimType, permission));
                }
            }
        }
    }
}
