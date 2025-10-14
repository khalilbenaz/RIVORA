namespace RVR.Framework.Domain.Entities;

/// <summary>
/// Entité avec audit de création et modification
/// </summary>
/// <typeparam name="TKey">Type de l'identifiant</typeparam>
public abstract class AuditedEntity<TKey> : Entity<TKey>
{
    /// <summary>
    /// Date et heure de création
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// Identifiant de l'utilisateur qui a créé l'entité
    /// </summary>
    public Guid? CreatorId { get; protected set; }

    /// <summary>
    /// Date et heure de la dernière modification
    /// </summary>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>
    /// Identifiant de l'utilisateur qui a modifié l'entité pour la dernière fois
    /// </summary>
    public Guid? LastModifierId { get; protected set; }

    /// <summary>
    /// Initialise les informations de création
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur créateur</param>
    protected void SetCreationInfo(Guid? userId = null)
    {
        CreatedAt = DateTime.UtcNow;
        CreatorId = userId;
    }

    /// <summary>
    /// Met à jour les informations de modification
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur qui modifie</param>
    protected void SetModificationInfo(Guid? userId = null)
    {
        UpdatedAt = DateTime.UtcNow;
        LastModifierId = userId;
    }
}
