namespace RVR.SaaS.Starter.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public string? TwoFactorSecret { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockedUntil { get; set; }
    public string Role { get; set; } = "User";
    public Guid TenantId { get; set; }
    
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
