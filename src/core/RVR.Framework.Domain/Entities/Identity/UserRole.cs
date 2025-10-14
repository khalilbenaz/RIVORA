namespace RVR.Framework.Domain.Entities.Identity;

/// <summary>
/// Association entre un utilisateur et un rôle
/// </summary>
public class UserRole : Entity<Guid>
{
    /// <summary>
    /// Identifiant du tenant
    /// </summary>
    public Guid? TenantId { get; private set; }

    /// <summary>
    /// Identifiant de l'utilisateur
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Identifiant du rôle
    /// </summary>
    public Guid RoleId { get; private set; }

    /// <summary>
    /// Utilisateur
    /// </summary>
    public virtual User User { get; private set; } = null!;

    /// <summary>
    /// Rôle
    /// </summary>
    public virtual Role Role { get; private set; } = null!;

    /// <summary>
    /// Constructeur privé pour EF Core
    /// </summary>
    private UserRole() { }

    /// <summary>
    /// Crée une nouvelle association utilisateur-rôle
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant</param>
    /// <param name="userId">Identifiant de l'utilisateur</param>
    /// <param name="roleId">Identifiant du rôle</param>
    public UserRole(Guid? tenantId, Guid userId, Guid roleId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("L'identifiant de l'utilisateur ne peut pas être vide.", nameof(userId));

        if (roleId == Guid.Empty)
            throw new ArgumentException("L'identifiant du rôle ne peut pas être vide.", nameof(roleId));

        Id = Guid.NewGuid();
        TenantId = tenantId;
        UserId = userId;
        RoleId = roleId;
    }
}
