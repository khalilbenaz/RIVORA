namespace KBA.Framework.Domain.Entities;

/// <summary>
/// Classe de base pour toutes les entités avec un identifiant
/// </summary>
/// <typeparam name="TKey">Type de l'identifiant</typeparam>
public abstract class Entity<TKey> : IEquatable<Entity<TKey>>
{
    /// <summary>
    /// Identifiant unique de l'entité
    /// </summary>
    public TKey Id { get; protected set; } = default!;

    private readonly List<object> _domainEvents = new();

    /// <summary>
    /// Liste des événements de domaine déclenchés par l'entité
    /// </summary>
    public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Ajoute un événement de domaine
    /// </summary>
    protected void AddDomainEvent(object domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Efface tous les événements de domaine
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Vérifie l'égalité entre deux entités basée sur leur identifiant
    /// </summary>
    public bool Equals(Entity<TKey>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;

        return Id?.Equals(other.Id) ?? false;
    }

    /// <summary>
    /// Vérifie l'égalité avec un objet
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is Entity<TKey> entity && Equals(entity);
    }

    /// <summary>
    /// Retourne le code de hachage de l'entité
    /// </summary>
    public override int GetHashCode()
    {
        return Id?.GetHashCode() ?? 0;
    }

    /// <summary>
    /// Opérateur d'égalité
    /// </summary>
    public static bool operator ==(Entity<TKey>? left, Entity<TKey>? right)
    {
        return left?.Equals(right) ?? right is null;
    }

    /// <summary>
    /// Opérateur d'inégalité
    /// </summary>
    public static bool operator !=(Entity<TKey>? left, Entity<TKey>? right)
    {
        return !(left == right);
    }
}
