using KBA.Framework.Domain.Entities;

namespace KBA.Framework.Domain.Entities.Security;

/// <summary>
/// Représente une clé API pour l'accès aux services (4.6)
/// </summary>
public class ApiKey : Entity<Guid>
{
    public ApiKey()
    {
        Id = Guid.NewGuid();
        CreatedAtUtc = DateTime.UtcNow;
        IsActive = true;
    }

    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid? TenantId { get; set; }
    public Guid? UserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public DateTime? LastUsedAtUtc { get; set; }
    public bool IsActive { get; set; }
}
