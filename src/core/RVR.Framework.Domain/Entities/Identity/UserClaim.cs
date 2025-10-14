namespace RVR.Framework.Domain.Entities.Identity;

/// <summary>
/// Claim (revendication) d'un utilisateur
/// </summary>
public class UserClaim : Entity<Guid>
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
    /// Type de claim
    /// </summary>
    public string ClaimType { get; private set; } = string.Empty;

    /// <summary>
    /// Valeur du claim
    /// </summary>
    public string? ClaimValue { get; private set; }

    /// <summary>
    /// Utilisateur
    /// </summary>
    public virtual User User { get; private set; } = null!;

    /// <summary>
    /// Constructeur privé pour EF Core
    /// </summary>
    private UserClaim() { }

    /// <summary>
    /// Crée un nouveau claim utilisateur
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant</param>
    /// <param name="userId">Identifiant de l'utilisateur</param>
    /// <param name="claimType">Type de claim</param>
    /// <param name="claimValue">Valeur du claim</param>
    public UserClaim(Guid? tenantId, Guid userId, string claimType, string? claimValue)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("L'identifiant de l'utilisateur ne peut pas être vide.", nameof(userId));

        if (string.IsNullOrWhiteSpace(claimType))
            throw new ArgumentException("Le type de claim ne peut pas être vide.", nameof(claimType));

        Id = Guid.NewGuid();
        TenantId = tenantId;
        UserId = userId;
        ClaimType = claimType.Trim();
        ClaimValue = claimValue?.Trim();
    }
}
