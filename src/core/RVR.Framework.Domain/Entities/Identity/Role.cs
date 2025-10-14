namespace RVR.Framework.Domain.Entities.Identity;

/// <summary>
/// Représente un rôle dans le système
/// </summary>
public class Role : FullAuditedEntity<Guid>
{
    /// <summary>
    /// Identifiant du tenant (null pour les rôles host)
    /// </summary>
    public Guid? TenantId { get; private set; }

    /// <summary>
    /// Nom du rôle
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Nom normalisé
    /// </summary>
    public string NormalizedName { get; private set; } = string.Empty;

    /// <summary>
    /// Indique si le rôle est par défaut (attribué automatiquement aux nouveaux utilisateurs)
    /// </summary>
    public bool IsDefault { get; private set; }

    /// <summary>
    /// Indique si le rôle est statique (ne peut pas être supprimé)
    /// </summary>
    public bool IsStatic { get; private set; }

    /// <summary>
    /// Description du rôle
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Utilisateurs ayant ce rôle
    /// </summary>
    public virtual ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

    /// <summary>
    /// Claims du rôle
    /// </summary>
    public virtual ICollection<RoleClaim> RoleClaims { get; private set; } = new List<RoleClaim>();

    /// <summary>
    /// Constructeur privé pour EF Core
    /// </summary>
    private Role() { }

    /// <summary>
    /// Crée un nouveau rôle
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant</param>
    /// <param name="name">Nom du rôle</param>
    /// <param name="isStatic">Indique si le rôle est statique</param>
    /// <param name="userId">Identifiant de l'utilisateur créateur</param>
    public Role(Guid? tenantId, string name, bool isStatic = false, Guid? userId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Le nom du rôle ne peut pas être vide.", nameof(name));

        Id = Guid.NewGuid();
        TenantId = tenantId;
        Name = name.Trim();
        NormalizedName = name.Trim().ToUpperInvariant();
        IsStatic = isStatic;
        IsDefault = false;
        SetCreationInfo(userId);
    }

    /// <summary>
    /// Définit le rôle comme rôle par défaut
    /// </summary>
    /// <param name="isDefault">True si le rôle est par défaut</param>
    /// <param name="userId">Identifiant de l'utilisateur qui modifie</param>
    public void SetAsDefault(bool isDefault, Guid? userId = null)
    {
        IsDefault = isDefault;
        SetModificationInfo(userId);
    }

    /// <summary>
    /// Met à jour le nom du rôle
    /// </summary>
    /// <param name="name">Nouveau nom</param>
    /// <param name="userId">Identifiant de l'utilisateur qui modifie</param>
    public void UpdateName(string name, Guid? userId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Le nom du rôle ne peut pas être vide.", nameof(name));

        Name = name.Trim();
        NormalizedName = name.Trim().ToUpperInvariant();
        SetModificationInfo(userId);
    }

    /// <summary>
    /// Met à jour la description
    /// </summary>
    /// <param name="description">Nouvelle description</param>
    /// <param name="userId">Identifiant de l'utilisateur qui modifie</param>
    public void UpdateDescription(string? description, Guid? userId = null)
    {
        Description = description?.Trim();
        SetModificationInfo(userId);
    }
}
