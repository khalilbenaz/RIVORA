using RVR.Framework.SMS.Models;

namespace RVR.Framework.SMS.Configuration;

/// <summary>
/// Root configuration options for the SMS module.
/// Bind to the "SMS" configuration section.
/// </summary>
public sealed class SmsOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "SMS";

    /// <summary>
    /// The default SMS provider to use when sending messages.
    /// </summary>
    public SmsProvider DefaultProvider { get; set; } = SmsProvider.Console;

    /// <summary>
    /// Default sender phone number or alphanumeric sender ID applied when <see cref="SmsMessage.From"/> is null.
    /// </summary>
    public string? DefaultFrom { get; set; }

    /// <summary>
    /// Twilio provider configuration.
    /// </summary>
    public TwilioOptions? Twilio { get; set; }

    /// <summary>
    /// Vonage (Nexmo) provider configuration.
    /// </summary>
    public VonageOptions? Vonage { get; set; }

    /// <summary>
    /// OVH SMS provider configuration.
    /// </summary>
    public OvhOptions? OVH { get; set; }

    /// <summary>
    /// Azure Communication Services provider configuration.
    /// </summary>
    public AzureOptions? Azure { get; set; }

    /// <summary>
    /// Maximum number of retry attempts for transient failures. Defaults to 3.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Base delay in milliseconds between retry attempts (exponential backoff). Defaults to 500ms.
    /// </summary>
    public int RetryBaseDelayMs { get; set; } = 500;
}

/// <summary>
/// Twilio-specific configuration options.
/// </summary>
public sealed class TwilioOptions
{
    /// <summary>
    /// Twilio Account SID.
    /// </summary>
    public string AccountSid { get; set; } = string.Empty;

    /// <summary>
    /// Twilio Auth Token.
    /// </summary>
    public string AuthToken { get; set; } = string.Empty;

    /// <summary>
    /// Default sender phone number for Twilio.
    /// </summary>
    public string? FromNumber { get; set; }

    /// <summary>
    /// Twilio REST API base URL. Defaults to the production endpoint.
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.twilio.com";
}

/// <summary>
/// Vonage (Nexmo) specific configuration options.
/// </summary>
public sealed class VonageOptions
{
    /// <summary>
    /// Vonage API Key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Vonage API Secret.
    /// </summary>
    public string ApiSecret { get; set; } = string.Empty;

    /// <summary>
    /// Default sender name or number for Vonage.
    /// </summary>
    public string? FromName { get; set; }

    /// <summary>
    /// Vonage REST API base URL. Defaults to the production endpoint.
    /// </summary>
    public string BaseUrl { get; set; } = "https://rest.nexmo.com";
}

/// <summary>
/// OVH SMS specific configuration options.
/// </summary>
public sealed class OvhOptions
{
    /// <summary>
    /// OVH Application Key.
    /// </summary>
    public string ApplicationKey { get; set; } = string.Empty;

    /// <summary>
    /// OVH Application Secret.
    /// </summary>
    public string ApplicationSecret { get; set; } = string.Empty;

    /// <summary>
    /// OVH Consumer Key.
    /// </summary>
    public string ConsumerKey { get; set; } = string.Empty;

    /// <summary>
    /// OVH SMS service name (e.g., "sms-xx12345-1").
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Default sender name for OVH.
    /// </summary>
    public string? SenderName { get; set; }

    /// <summary>
    /// OVH API base URL. Defaults to the EU endpoint.
    /// </summary>
    public string BaseUrl { get; set; } = "https://eu.api.ovh.com/1.0";
}

/// <summary>
/// Azure Communication Services specific configuration options.
/// </summary>
public sealed class AzureOptions
{
    /// <summary>
    /// Azure Communication Services connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Azure Communication Services endpoint URL.
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Azure Communication Services access key.
    /// </summary>
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>
    /// Default sender phone number for Azure.
    /// </summary>
    public string? FromNumber { get; set; }
}
