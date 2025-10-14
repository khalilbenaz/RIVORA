namespace RVR.Framework.Domain.Events;

/// <summary>
/// Interface de base pour tous les événements de domaine
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Date et heure de création de l'événement
    /// </summary>
    DateTime OccurredOn { get; }
}
