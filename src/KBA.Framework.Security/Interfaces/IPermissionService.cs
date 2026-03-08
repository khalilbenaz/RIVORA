namespace KBA.Framework.Security.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Defines the contract for permission service operations.
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Checks if a user has a specific permission.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="permission">The permission to check (e.g., "orders.write").</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if the user has the permission.</returns>
    Task<bool> HasPermissionAsync(string userId, string permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has any of the specified permissions.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="permissions">The permissions to check.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if the user has at least one of the permissions.</returns>
    Task<bool> HasAnyPermissionAsync(
        string userId,
        IEnumerable<string> permissions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has all of the specified permissions.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="permissions">The permissions to check.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if the user has all of the permissions.</returns>
    Task<bool> HasAllPermissionsAsync(
        string userId,
        IEnumerable<string> permissions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all permissions for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of permission strings.</returns>
    Task<IEnumerable<string>> GetPermissionsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Grants a permission to a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="permission">The permission to grant.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task GrantPermissionAsync(string userId, string permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a permission from a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="permission">The permission to revoke.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RevokePermissionAsync(string userId, string permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all available permissions in the system.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of all available permissions.</returns>
    Task<IEnumerable<PermissionDefinition>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a permission definition.
    /// </summary>
    /// <param name="permission">The permission definition.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RegisterPermissionAsync(PermissionDefinition permission, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a permission definition.
/// </summary>
public class PermissionDefinition
{
    /// <summary>
    /// Gets or sets the permission name (e.g., "orders.write").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the permission.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the permission.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the category/group of the permission (e.g., "Orders", "Users").
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parent permission name (for hierarchical permissions).
    /// </summary>
    public string? Parent { get; set; }

    /// <summary>
    /// Gets or sets whether this permission is enabled by default.
    /// </summary>
    public bool IsEnabledByDefault { get; set; } = true;

    /// <summary>
    /// Gets or sets whether this permission is a system permission (cannot be revoked).
    /// </summary>
    public bool IsSystem { get; set; } = false;

    /// <summary>
    /// Gets or sets additional metadata for the permission.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionDefinition"/> class.
    /// </summary>
    public PermissionDefinition()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionDefinition"/> class.
    /// </summary>
    /// <param name="name">The permission name.</param>
    /// <param name="displayName">The display name.</param>
    /// <param name="category">The category.</param>
    public PermissionDefinition(string name, string displayName, string category)
    {
        Name = name;
        DisplayName = displayName;
        Category = category;
    }
}
