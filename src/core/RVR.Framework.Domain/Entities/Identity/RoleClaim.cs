namespace RVR.Framework.Domain.Entities.Identity;

/// <summary>
/// Claim (revendication) d'un rôle
/// </summary>
public class RoleClaim : Entity<Guid>
{
    /// <summary>
    /// Identifiant du tenant
    /// </summary>
    public Guid? TenantId { get; private set; }

    /// <summary>
    /// Identifiant du rôle
    /// </summary>
    public Guid RoleId { get; private set; }

    /// <summary>
    /// Type de claim
    /// </summary>
    public string ClaimType { get; private set; } = string.Empty;

    /// <summary>
    /// Valeur du claim
    /// </summary>
    public string? ClaimValue { get; private set; }

    /// <summary>
    /// Rôle
    /// </summary>
    public virtual Role Role { get; private set; } = null!;

    /// <summary>
    /// Constructeur privé pour EF Core
    /// </summary>
    private RoleClaim() { }

    /// <summary>
    /// Crée un nouveau claim de rôle
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant</param>
    /// <param name="roleId">Identifiant du rôle</param>
    /// <param name="claimType">Type de claim</param>
    /// <param name="claimValue">Valeur du claim</param>
    public RoleClaim(Guid? tenantId, Guid roleId, string claimType, string? claimValue)
    {
        if (roleId == Guid.Empty)
            throw new ArgumentException("L'identifiant du rôle ne peut pas être vide.", nameof(roleId));

        if (string.IsNullOrWhiteSpace(claimType))
            throw new ArgumentException("Le type de claim ne peut pas être vide.", nameof(claimType));

        Id = Guid.NewGuid();
        TenantId = tenantId;
        RoleId = roleId;
        ClaimType = claimType.Trim();
        ClaimValue = claimValue?.Trim();
    }
}
