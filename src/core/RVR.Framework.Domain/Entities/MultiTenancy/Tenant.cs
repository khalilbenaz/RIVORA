namespace RVR.Framework.Domain.Entities.MultiTenancy;

/// <summary>
/// Représente un locataire (tenant) dans le système multi-tenant
/// </summary>
public class Tenant : FullAuditedEntity<Guid>
{
    /// <summary>
    /// Nom du tenant
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Nom normalisé pour les recherches
    /// </summary>
    public string NormalizedName { get; private set; } = string.Empty;

    /// <summary>
    /// Indique si le tenant est actif
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Chaînes de connexion associées au tenant
    /// </summary>
    public virtual ICollection<TenantConnectionString> ConnectionStrings { get; private set; } = new List<TenantConnectionString>();

    /// <summary>
    /// Constructeur privé pour EF Core
    /// </summary>
    private Tenant() { }

    /// <summary>
    /// Crée un nouveau tenant
    /// </summary>
    /// <param name="name">Nom du tenant</param>
    /// <param name="userId">Identifiant de l'utilisateur créateur</param>
    public Tenant(string name, Guid? userId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Le nom du tenant ne peut pas être vide.", nameof(name));

        if (name.Length > RVRConsts.MaxNameLength)
            throw new ArgumentException($"Le nom du tenant ne peut pas dépasser {RVRConsts.MaxNameLength} caractères.", nameof(name));

        Id = Guid.NewGuid();
        Name = name.Trim();
        NormalizedName = name.Trim().ToUpperInvariant();
        IsActive = true;
        SetCreationInfo(userId);
    }

    /// <summary>
    /// Active le tenant
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur qui active</param>
    public void Activate(Guid? userId = null)
    {
        IsActive = true;
        SetModificationInfo(userId);
    }

    /// <summary>
    /// Désactive le tenant
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur qui désactive</param>
    public void Deactivate(Guid? userId = null)
    {
        IsActive = false;
        SetModificationInfo(userId);
    }

    /// <summary>
    /// Met à jour le nom du tenant
    /// </summary>
    /// <param name="name">Nouveau nom</param>
    /// <param name="userId">Identifiant de l'utilisateur qui modifie</param>
    public void UpdateName(string name, Guid? userId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Le nom du tenant ne peut pas être vide.", nameof(name));

        if (name.Length > RVRConsts.MaxNameLength)
            throw new ArgumentException($"Le nom du tenant ne peut pas dépasser {RVRConsts.MaxNameLength} caractères.", nameof(name));

        Name = name.Trim();
        NormalizedName = name.Trim().ToUpperInvariant();
        SetModificationInfo(userId);
    }
}
