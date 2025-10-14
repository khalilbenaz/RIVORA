namespace RVR.Framework.Domain.Entities.Organization;

/// <summary>
/// Unité organisationnelle (département, équipe, etc.)
/// </summary>
public class OrganizationUnit : FullAuditedEntity<Guid>
{
    /// <summary>
    /// Identifiant du tenant
    /// </summary>
    public Guid? TenantId { get; private set; }

    /// <summary>
    /// Identifiant de l'unité parente
    /// </summary>
    public Guid? ParentId { get; private set; }

    /// <summary>
    /// Code hiérarchique (ex: 00001.00002.00003)
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// Nom d'affichage
    /// </summary>
    public string DisplayName { get; private set; } = string.Empty;

    /// <summary>
    /// Unité parente
    /// </summary>
    public virtual OrganizationUnit? Parent { get; private set; }

    /// <summary>
    /// Unités enfants
    /// </summary>
    public virtual ICollection<OrganizationUnit> Children { get; private set; } = new List<OrganizationUnit>();

    /// <summary>
    /// Utilisateurs dans cette unité
    /// </summary>
    public virtual ICollection<UserOrganizationUnit> UserOrganizationUnits { get; private set; } = new List<UserOrganizationUnit>();

    /// <summary>
    /// Constructeur privé pour EF Core
    /// </summary>
    private OrganizationUnit() { }

    /// <summary>
    /// Crée une nouvelle unité organisationnelle
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant</param>
    /// <param name="displayName">Nom d'affichage</param>
    /// <param name="code">Code hiérarchique</param>
    /// <param name="parentId">Identifiant de l'unité parente</param>
    /// <param name="userId">Identifiant de l'utilisateur créateur</param>
    public OrganizationUnit(Guid? tenantId, string displayName, string code, Guid? parentId = null, Guid? userId = null)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Le nom d'affichage ne peut pas être vide.", nameof(displayName));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Le code ne peut pas être vide.", nameof(code));

        Id = Guid.NewGuid();
        TenantId = tenantId;
        DisplayName = displayName.Trim();
        Code = code.Trim();
        ParentId = parentId;
        SetCreationInfo(userId);
    }

    /// <summary>
    /// Met à jour le nom d'affichage
    /// </summary>
    public void UpdateDisplayName(string displayName, Guid? userId = null)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Le nom d'affichage ne peut pas être vide.", nameof(displayName));

        DisplayName = displayName.Trim();
        SetModificationInfo(userId);
    }
}
