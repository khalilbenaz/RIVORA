namespace RVR.Framework.Domain.Entities.Identity;

/// <summary>
/// Login externe d'un utilisateur (OAuth, etc.)
/// </summary>
public class UserLogin : Entity<Guid>
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
    /// Fournisseur de login (Google, Facebook, etc.)
    /// </summary>
    public string LoginProvider { get; private set; } = string.Empty;

    /// <summary>
    /// Clé du fournisseur
    /// </summary>
    public string ProviderKey { get; private set; } = string.Empty;

    /// <summary>
    /// Nom d'affichage du fournisseur
    /// </summary>
    public string? ProviderDisplayName { get; private set; }

    /// <summary>
    /// Utilisateur
    /// </summary>
    public virtual User User { get; private set; } = null!;

    /// <summary>
    /// Constructeur privé pour EF Core
    /// </summary>
    private UserLogin() { }

    /// <summary>
    /// Crée un nouveau login externe
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant</param>
    /// <param name="userId">Identifiant de l'utilisateur</param>
    /// <param name="loginProvider">Fournisseur de login</param>
    /// <param name="providerKey">Clé du fournisseur</param>
    /// <param name="providerDisplayName">Nom d'affichage du fournisseur</param>
    public UserLogin(Guid? tenantId, Guid userId, string loginProvider, string providerKey, string? providerDisplayName = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("L'identifiant de l'utilisateur ne peut pas être vide.", nameof(userId));

        if (string.IsNullOrWhiteSpace(loginProvider))
            throw new ArgumentException("Le fournisseur de login ne peut pas être vide.", nameof(loginProvider));

        if (string.IsNullOrWhiteSpace(providerKey))
            throw new ArgumentException("La clé du fournisseur ne peut pas être vide.", nameof(providerKey));

        Id = Guid.NewGuid();
        TenantId = tenantId;
        UserId = userId;
        LoginProvider = loginProvider.Trim();
        ProviderKey = providerKey.Trim();
        ProviderDisplayName = providerDisplayName?.Trim();
    }
}
