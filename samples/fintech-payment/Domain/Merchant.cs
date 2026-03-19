namespace RVR.Fintech.Payment.Domain;

public sealed class Merchant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public Guid? ApiKeyId { get; set; }
    public string? WebhookUrl { get; set; }
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
