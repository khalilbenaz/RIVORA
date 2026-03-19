namespace RVR.Framework.Webhooks.Incoming;

/// <summary>
/// Log entry for a received incoming webhook.
/// </summary>
public class IncomingWebhookLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string ConfigId { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = "POST";
    public string? EventType { get; set; }
    public string Headers { get; set; } = string.Empty; // JSON serialized
    public string Payload { get; set; } = string.Empty;
    public int? StatusCode { get; set; }
    public bool SignatureValid { get; set; }
    public string? Error { get; set; }
    public string Status { get; set; } = "received"; // received, processing, processed, failed
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
}
