namespace RVR.Framework.Security.Attributes;

using System;
using System.Threading.Tasks;
using RVR.Framework.Security.Interfaces;
using Microsoft.AspNetCore.Authorization;

/// <summary>
/// Authorization attribute that requires specific permissions.
/// Can be applied to controllers, actions, or Razor pages.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Gets or sets the required permission.
    /// </summary>
    public string Permission { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the required permissions (all must be present).
    /// </summary>
    public string[]? Permissions { get; init; }

    /// <summary>
    /// Gets or sets whether any of the permissions is sufficient (OR logic).
    /// Default is false (AND logic - all permissions required).
    /// </summary>
    public bool Any { get; init; } = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequirePermissionAttribute"/> class.
    /// </summary>
    /// <param name="permission">The required permission.</param>
    public RequirePermissionAttribute(string permission)
    {
        Permission = permission;
        Policy = $"Permission:{permission}";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RequirePermissionAttribute"/> class with multiple permissions.
    /// </summary>
    /// <param name="permissions">The required permissions.</param>
    public RequirePermissionAttribute(params string[] permissions)
    {
        Permissions = permissions;
        Policy = $"Permissions:{string.Join(",", permissions)}";
    }
}

/// <summary>
/// Authorization handler for permission-based authorization.
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionService _permissionService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionAuthorizationHandler"/> class.
    /// </summary>
    /// <param name="permissionService">The permission service.</param>
    public PermissionAuthorizationHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
    }

    /// <inheritdoc/>
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userId = context.User.FindFirst(c =>
            c.Type == System.Security.Claims.ClaimTypes.NameIdentifier ||
            c.Type == "sub" ||
            c.Type == "user_id")?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            return;
        }

        var hasPermission = await _permissionService.HasPermissionAsync(
            userId,
            requirement.Permission,
            context.Resource is CancellationToken ct ? ct : CancellationToken.None);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}

/// <summary>
/// Authorization requirement for permission-based authorization.
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Gets the required permission.
    /// </summary>
    public string Permission { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionRequirement"/> class.
    /// </summary>
    /// <param name="permission">The required permission.</param>
    public PermissionRequirement(string permission)
    {
        Permission = permission ?? throw new ArgumentNullException(nameof(permission));
    }
}
