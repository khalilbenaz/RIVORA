namespace RVR.Framework.Domain.Entities.Auditing;

/// <summary>
/// Changement d'une entité (création, modification, suppression)
/// </summary>
public class EntityChange : Entity<Guid>
{
    /// <summary>
    /// Identifiant du log d'audit
    /// </summary>
    public Guid AuditLogId { get; private set; }

    /// <summary>
    /// Identifiant de l'entité modifiée
    /// </summary>
    public string EntityId { get; private set; } = string.Empty;

    /// <summary>
    /// Nom complet du type d'entité
    /// </summary>
    public string EntityTypeFullName { get; private set; } = string.Empty;

    /// <summary>
    /// Type de changement (Created, Updated, Deleted)
    /// </summary>
    public string ChangeType { get; private set; } = string.Empty;

    /// <summary>
    /// Date et heure du changement
    /// </summary>
    public DateTime ChangeTime { get; private set; }

    /// <summary>
    /// Log d'audit parent
    /// </summary>
    public virtual AuditLog AuditLog { get; private set; } = null!;

    /// <summary>
    /// Changements de propriétés
    /// </summary>
    public virtual ICollection<EntityPropertyChange> PropertyChanges { get; private set; } = new List<EntityPropertyChange>();

    /// <summary>
    /// Constructeur privé pour EF Core
    /// </summary>
    private EntityChange() { }

    /// <summary>
    /// Crée un nouveau changement d'entité
    /// </summary>
    /// <param name="auditLogId">Identifiant du log d'audit</param>
    /// <param name="entityId">Identifiant de l'entité</param>
    /// <param name="entityTypeFullName">Type de l'entité</param>
    /// <param name="changeType">Type de changement</param>
    public EntityChange(Guid auditLogId, string entityId, string entityTypeFullName, string changeType)
    {
        if (auditLogId == Guid.Empty)
            throw new ArgumentException("L'identifiant du log d'audit ne peut pas être vide.", nameof(auditLogId));

        if (string.IsNullOrWhiteSpace(entityId))
            throw new ArgumentException("L'identifiant de l'entité ne peut pas être vide.", nameof(entityId));

        if (string.IsNullOrWhiteSpace(entityTypeFullName))
            throw new ArgumentException("Le type d'entité ne peut pas être vide.", nameof(entityTypeFullName));

        if (string.IsNullOrWhiteSpace(changeType))
            throw new ArgumentException("Le type de changement ne peut pas être vide.", nameof(changeType));

        Id = Guid.NewGuid();
        AuditLogId = auditLogId;
        EntityId = entityId.Trim();
        EntityTypeFullName = entityTypeFullName.Trim();
        ChangeType = changeType.Trim();
        ChangeTime = DateTime.UtcNow;
    }
}
