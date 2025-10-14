using RVR.Framework.Domain.Events;

namespace RVR.Framework.Domain.Entities;

/// <summary>
/// Racine d'agrégat pour DDD avec support des événements de domaine
/// </summary>
/// <typeparam name="TKey">Type de l'identifiant</typeparam>
public abstract class AggregateRoot<TKey> : FullAuditedEntity<TKey>
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Liste des événements de domaine en lecture seule
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Ajoute un événement de domaine
    /// </summary>
    /// <param name="domainEvent">Événement à ajouter</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Supprime un événement de domaine
    /// </summary>
    /// <param name="domainEvent">Événement à supprimer</param>
    protected void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Efface tous les événements de domaine
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
