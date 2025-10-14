namespace RVR.Framework.Domain.Entities;

/// <summary>
/// Interface pour les entités supportant la suppression logique (Soft Delete)
/// </summary>
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAtUtc { get; set; }
}
