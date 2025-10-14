namespace RVR.Framework.Security.Services;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RVR.Framework.Security.Interfaces;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for managing user permissions and role-based access control.
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly IPermissionStore _store;
    private readonly ILogger<PermissionService> _logger;
    private readonly ConcurrentDictionary<string, PermissionDefinition> _registeredPermissions = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionService"/> class.
    /// </summary>
    /// <param name="store">The permission store.</param>
    /// <param name="logger">The logger.</param>
    public PermissionService(IPermissionStore store, ILogger<PermissionService> logger)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<bool> HasPermissionAsync(string userId, string permission, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(permission))
        {
            return false;
        }

        var permissions = await GetPermissionsAsync(userId, cancellationToken);
        return permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public async Task<bool> HasAnyPermissionAsync(
        string userId,
        IEnumerable<string> permissions,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        var permissionsList = permissions?.ToList() ?? new List<string>();

        if (permissionsList.Count == 0)
        {
            return false;
        }

        var userPermissions = await GetPermissionsAsync(userId, cancellationToken);
        return userPermissions.Intersect(permissionsList, StringComparer.OrdinalIgnoreCase).Any();
    }

    /// <inheritdoc/>
    public async Task<bool> HasAllPermissionsAsync(
        string userId,
        IEnumerable<string> permissions,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        var permissionsList = permissions?.ToList() ?? new List<string>();

        if (permissionsList.Count == 0)
        {
            return true;
        }

        var userPermissions = await GetPermissionsAsync(userId, cancellationToken);
        return permissionsList.All(p => userPermissions.Contains(p, StringComparer.OrdinalIgnoreCase));
    }

    /// <inheritdoc/>
    public Task<IEnumerable<string>> GetPermissionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Task.FromResult(Enumerable.Empty<string>());
        }

        return _store.GetPermissionsAsync(userId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task GrantPermissionAsync(string userId, string permission, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(permission))
        {
            throw new ArgumentException("Permission cannot be empty.", nameof(permission));
        }

        await _store.GrantPermissionAsync(userId, permission, cancellationToken);

        _logger.LogInformation(
            "Granted permission {Permission} to user {UserId}",
            permission, userId);
    }

    /// <inheritdoc/>
    public async Task RevokePermissionAsync(string userId, string permission, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(permission))
        {
            throw new ArgumentException("Permission cannot be empty.", nameof(permission));
        }

        // Check if this is a system permission
        if (_registeredPermissions.TryGetValue(permission, out var definition) && definition.IsSystem)
        {
            _logger.LogWarning(
                "Attempted to revoke system permission {Permission} from user {UserId}",
                permission, userId);
            return;
        }

        await _store.RevokePermissionAsync(userId, permission, cancellationToken);

        _logger.LogInformation(
            "Revoked permission {Permission} from user {UserId}",
            permission, userId);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<PermissionDefinition>> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
    {
        var permissions = _registeredPermissions.Values.ToList();

        // Also get any permissions from the store that aren't registered
        var storedPermissions = await _store.GetAllPermissionsAsync(cancellationToken);

        foreach (var permission in storedPermissions)
        {
            if (!_registeredPermissions.ContainsKey(permission))
            {
                permissions.Add(new PermissionDefinition
                {
                    Name = permission,
                    DisplayName = permission,
                    Category = "Unknown"
                });
            }
        }

        return permissions.OrderBy(p => p.Category).ThenBy(p => p.Name);
    }

    /// <inheritdoc/>
    public Task RegisterPermissionAsync(PermissionDefinition permission, CancellationToken cancellationToken = default)
    {
        if (permission == null)
        {
            throw new ArgumentNullException(nameof(permission));
        }

        if (string.IsNullOrWhiteSpace(permission.Name))
        {
            throw new ArgumentException("Permission name cannot be empty.", nameof(permission));
        }

        _registeredPermissions[permission.Name] = permission;

        _logger.LogDebug("Registered permission {Permission}", permission.Name);

        return Task.CompletedTask;
    }
}

/// <summary>
/// Defines the contract for permission storage operations.
/// </summary>
public interface IPermissionStore
{
    /// <summary>
    /// Gets all permissions for a user.
    /// </summary>
    Task<IEnumerable<string>> GetPermissionsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Grants a permission to a user.
    /// </summary>
    Task GrantPermissionAsync(string userId, string permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a permission from a user.
    /// </summary>
    Task RevokePermissionAsync(string userId, string permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all registered permissions in the system.
    /// </summary>
    Task<IEnumerable<string>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);
}
