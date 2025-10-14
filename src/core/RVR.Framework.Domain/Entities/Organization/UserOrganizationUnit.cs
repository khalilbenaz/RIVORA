using RVR.Framework.Domain.Entities.Identity;

namespace RVR.Framework.Domain.Entities.Organization;

/// <summary>
/// Association entre un utilisateur et une unité organisationnelle
/// </summary>
public class UserOrganizationUnit : Entity<Guid>
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
    /// Identifiant de l'unité organisationnelle
    /// </summary>
    public Guid OrganizationUnitId { get; private set; }

    /// <summary>
    /// Utilisateur
    /// </summary>
    public virtual User User { get; private set; } = null!;

    /// <summary>
    /// Unité organisationnelle
    /// </summary>
    public virtual OrganizationUnit OrganizationUnit { get; private set; } = null!;

    /// <summary>
    /// Constructeur privé pour EF Core
    /// </summary>
    private UserOrganizationUnit() { }

    /// <summary>
    /// Crée une nouvelle association utilisateur-unité organisationnelle
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant</param>
    /// <param name="userId">Identifiant de l'utilisateur</param>
    /// <param name="organizationUnitId">Identifiant de l'unité organisationnelle</param>
    public UserOrganizationUnit(Guid? tenantId, Guid userId, Guid organizationUnitId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("L'identifiant de l'utilisateur ne peut pas être vide.", nameof(userId));

        if (organizationUnitId == Guid.Empty)
            throw new ArgumentException("L'identifiant de l'unité organisationnelle ne peut pas être vide.", nameof(organizationUnitId));

        Id = Guid.NewGuid();
        TenantId = tenantId;
        UserId = userId;
        OrganizationUnitId = organizationUnitId;
    }
}
