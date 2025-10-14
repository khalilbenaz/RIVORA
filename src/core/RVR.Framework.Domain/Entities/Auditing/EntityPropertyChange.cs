namespace RVR.Framework.Domain.Entities.Auditing;

/// <summary>
/// Changement d'une propriété d'entité
/// </summary>
public class EntityPropertyChange : Entity<Guid>
{
    /// <summary>
    /// Identifiant du changement d'entité parent
    /// </summary>
    public Guid EntityChangeId { get; private set; }

    /// <summary>
    /// Nom de la propriété
    /// </summary>
    public string PropertyName { get; private set; } = string.Empty;

    /// <summary>
    /// Nouvelle valeur (JSON)
    /// </summary>
    public string? NewValue { get; private set; }

    /// <summary>
    /// Valeur originale (JSON)
    /// </summary>
    public string? OriginalValue { get; private set; }

    /// <summary>
    /// Type de la propriété
    /// </summary>
    public string? PropertyTypeFullName { get; private set; }

    /// <summary>
    /// Changement d'entité parent
    /// </summary>
    public virtual EntityChange EntityChange { get; private set; } = null!;

    /// <summary>
    /// Constructeur privé pour EF Core
    /// </summary>
    private EntityPropertyChange() { }

    /// <summary>
    /// Crée un nouveau changement de propriété
    /// </summary>
    /// <param name="entityChangeId">Identifiant du changement d'entité</param>
    /// <param name="propertyName">Nom de la propriété</param>
    /// <param name="newValue">Nouvelle valeur</param>
    /// <param name="originalValue">Valeur originale</param>
    /// <param name="propertyTypeFullName">Type de la propriété</param>
    public EntityPropertyChange(Guid entityChangeId, string propertyName, string? newValue, string? originalValue, string? propertyTypeFullName = null)
    {
        if (entityChangeId == Guid.Empty)
            throw new ArgumentException("L'identifiant du changement d'entité ne peut pas être vide.", nameof(entityChangeId));

        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentException("Le nom de la propriété ne peut pas être vide.", nameof(propertyName));

        Id = Guid.NewGuid();
        EntityChangeId = entityChangeId;
        PropertyName = propertyName.Trim();
        NewValue = newValue?.Trim();
        OriginalValue = originalValue?.Trim();
        PropertyTypeFullName = propertyTypeFullName?.Trim();
    }
}
