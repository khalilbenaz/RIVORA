namespace RVR.Framework.Domain.Entities.Permissions;

/// <summary>
/// Définit une permission dans le système
/// </summary>
public class Permission : Entity<Guid>
{
    /// <summary>
    /// Nom unique de la permission
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Nom d'affichage
    /// </summary>
    public string DisplayName { get; private set; } = string.Empty;

    /// <summary>
    /// Groupe de permissions (ex: Users, Products, etc.)
    /// </summary>
    public string? GroupName { get; private set; }

    /// <summary>
    /// Identifiant de la permission parente (pour hiérarchie)
    /// </summary>
    public Guid? ParentId { get; private set; }

    /// <summary>
    /// Permission parente
    /// </summary>
    public virtual Permission? Parent { get; private set; }

    /// <summary>
    /// Permissions enfants
    /// </summary>
    public virtual ICollection<Permission> Children { get; private set; } = new List<Permission>();

    /// <summary>
    /// Constructeur privé pour EF Core
    /// </summary>
    private Permission() { }

    /// <summary>
    /// Crée une nouvelle permission
    /// </summary>
    /// <param name="name">Nom unique</param>
    /// <param name="displayName">Nom d'affichage</param>
    /// <param name="groupName">Groupe</param>
    /// <param name="parentId">Identifiant de la permission parente</param>
    public Permission(string name, string displayName, string? groupName = null, Guid? parentId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Le nom de la permission ne peut pas être vide.", nameof(name));

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Le nom d'affichage ne peut pas être vide.", nameof(displayName));

        Id = Guid.NewGuid();
        Name = name.Trim();
        DisplayName = displayName.Trim();
        GroupName = groupName?.Trim();
        ParentId = parentId;
    }
}
