namespace RVR.Fintech.Payment.Domain;

public enum PaymentStatus
{
    Pending,
    Completed,
    Failed,
    Refunded
}

public sealed class PaymentTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EUR";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public Guid MerchantId { get; set; }
    public string? IdempotencyKey { get; set; }
    public string? Description { get; set; }
    public string? CustomerEmail { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
