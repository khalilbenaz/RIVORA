namespace RVR.SaaS.Starter.Domain.Entities;

public class FeatureFlag : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public Guid? TenantId { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
