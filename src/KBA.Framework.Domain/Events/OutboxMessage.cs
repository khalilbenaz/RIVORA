using KBA.Framework.Domain.Entities;

namespace KBA.Framework.Domain.Events;

/// <summary>
/// Représente un message dans la table Outbox pour la publication fiable d'événements
/// </summary>
public class OutboxMessage : Entity<Guid>
{
    public OutboxMessage()
    {
        Id = Guid.NewGuid();
        OccurredOnUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Type complet de l'événement (pour la désérialisation)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Contenu sérialisé de l'événement (JSON)
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Date et heure de l'événement
    /// </summary>
    public DateTime OccurredOnUtc { get; set; }

    /// <summary>
    /// Date et heure du traitement (null si non traité)
    /// </summary>
    public DateTime? ProcessedOnUtc { get; set; }

    /// <summary>
    /// Message d'erreur en cas d'échec du traitement
    /// </summary>
    public string? Error { get; set; }
}
