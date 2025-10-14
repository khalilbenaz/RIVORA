namespace RVR.Framework.Webhooks;

/// <summary>
/// Configuration options for the webhook system.
/// </summary>
public class WebhookOptions
{
    /// <summary>Gets or sets the default maximum number of retry attempts for failed deliveries.</summary>
    public int DefaultMaxRetries { get; set; } = 3;

    /// <summary>Gets or sets the HTTP request timeout in seconds.</summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>Gets or sets the HTTP header name used for the HMAC-SHA256 signature.</summary>
    public string SignatureHeader { get; set; } = "X-Webhook-Signature";

    /// <summary>Gets or sets the allowed URL schemes for callback URLs. Defaults to ["https"].</summary>
    public string[] AllowedSchemes { get; set; } = ["https"];

    /// <summary>Gets or sets whether to block callback URLs targeting private/loopback networks. Defaults to true.</summary>
    public bool BlockPrivateNetworks { get; set; } = true;
}
