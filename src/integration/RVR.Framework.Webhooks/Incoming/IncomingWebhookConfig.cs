namespace RVR.Framework.Webhooks.Incoming;

/// <summary>
/// Configuration for an incoming webhook endpoint.
/// </summary>
public class IncomingWebhookConfig
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty; // e.g., "stripe", "github", "custom"
    public string? SignatureHeader { get; set; } // e.g., "Stripe-Signature", "X-Hub-Signature-256"
    public string? Secret { get; set; }
    public string SignatureAlgorithm { get; set; } = "hmac-sha256"; // hmac-sha256, hmac-sha1
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
