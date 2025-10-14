namespace RVR.Framework.Domain.Entities;

/// <summary>
/// Entité avec audit complet incluant la suppression logique (soft delete)
/// </summary>
/// <typeparam name="TKey">Type de l'identifiant</typeparam>
public abstract class FullAuditedEntity<TKey> : AuditedEntity<TKey>
{
    /// <summary>
    /// Indique si l'entité a été supprimée logiquement
    /// </summary>
    public bool IsDeleted { get; protected set; }

    /// <summary>
    /// Date et heure de la suppression
    /// </summary>
    public DateTime? DeletedAt { get; protected set; }

    /// <summary>
    /// Identifiant de l'utilisateur qui a supprimé l'entité
    /// </summary>
    public Guid? DeleterId { get; protected set; }

    /// <summary>
    /// Marque l'entité comme supprimée (soft delete)
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur qui supprime</param>
    public virtual void Delete(Guid? userId = null)
    {
        if (IsDeleted) return;

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeleterId = userId;
    }

    /// <summary>
    /// Restaure l'entité supprimée
    /// </summary>
    public virtual void Restore()
    {
        if (!IsDeleted) return;

        IsDeleted = false;
        DeletedAt = null;
        DeleterId = null;
    }
}
