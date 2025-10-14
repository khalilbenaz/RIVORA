namespace RVR.Framework.Domain.Entities.Permissions;

/// <summary>
/// Attribution d'une permission à un utilisateur ou rôle
/// </summary>
public class PermissionGrant : Entity<Guid>
{
    /// <summary>
    /// Identifiant du tenant
    /// </summary>
    public Guid? TenantId { get; private set; }

    /// <summary>
    /// Nom de la permission
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Type de fournisseur (Role, User)
    /// </summary>
    public string ProviderName { get; private set; } = string.Empty;

    /// <summary>
    /// Clé du fournisseur (Id du role ou de l'utilisateur)
    /// </summary>
    public string ProviderKey { get; private set; } = string.Empty;

    /// <summary>
    /// Constructeur privé pour EF Core
    /// </summary>
    private PermissionGrant() { }

    /// <summary>
    /// Crée une nouvelle attribution de permission
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant</param>
    /// <param name="name">Nom de la permission</param>
    /// <param name="providerName">Type de fournisseur</param>
    /// <param name="providerKey">Clé du fournisseur</param>
    public PermissionGrant(Guid? tenantId, string name, string providerName, string providerKey)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Le nom de la permission ne peut pas être vide.", nameof(name));

        if (string.IsNullOrWhiteSpace(providerName))
            throw new ArgumentException("Le type de fournisseur ne peut pas être vide.", nameof(providerName));

        if (string.IsNullOrWhiteSpace(providerKey))
            throw new ArgumentException("La clé du fournisseur ne peut pas être vide.", nameof(providerKey));

        Id = Guid.NewGuid();
        TenantId = tenantId;
        Name = name.Trim();
        ProviderName = providerName.Trim();
        ProviderKey = providerKey.Trim();
    }
}
