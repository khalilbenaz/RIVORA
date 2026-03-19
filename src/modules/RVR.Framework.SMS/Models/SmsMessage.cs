namespace RVR.Framework.SMS.Models;

/// <summary>
/// Represents an SMS message to be sent.
/// </summary>
/// <param name="To">The destination phone number in E.164 format (e.g., +33612345678).</param>
/// <param name="Body">The text body of the SMS message.</param>
/// <param name="From">Optional sender phone number or alphanumeric sender ID.</param>
/// <param name="Metadata">Optional metadata dictionary for provider-specific extensions.</param>
public sealed record SmsMessage(
    string To,
    string Body,
    string? From = null,
    Dictionary<string, string>? Metadata = null);

/// <summary>
/// Represents the result of an SMS send operation.
/// </summary>
/// <param name="Success">Whether the message was accepted by the provider.</param>
/// <param name="MessageId">The provider-assigned message identifier.</param>
/// <param name="Error">Error description if the send failed.</param>
/// <param name="Provider">The provider that handled the message.</param>
public sealed record SmsResult(
    bool Success,
    string? MessageId = null,
    string? Error = null,
    SmsProvider Provider = SmsProvider.None);

/// <summary>
/// Represents the delivery status of a previously sent SMS message.
/// </summary>
/// <param name="MessageId">The provider-assigned message identifier.</param>
/// <param name="Status">The current delivery status.</param>
/// <param name="DeliveredAt">The timestamp when the message was delivered, if available.</param>
public sealed record SmsStatus(
    string MessageId,
    SmsDeliveryStatus Status,
    DateTime? DeliveredAt = null);

/// <summary>
/// SMS delivery status values.
/// </summary>
public enum SmsDeliveryStatus
{
    /// <summary>Message is queued and pending delivery.</summary>
    Pending,
    /// <summary>Message has been sent to the carrier.</summary>
    Sent,
    /// <summary>Message has been delivered to the recipient.</summary>
    Delivered,
    /// <summary>Message delivery failed.</summary>
    Failed,
    /// <summary>Delivery status is unknown.</summary>
    Unknown
}

/// <summary>
/// Supported SMS providers.
/// </summary>
public enum SmsProvider
{
    /// <summary>No provider configured.</summary>
    None,
    /// <summary>Twilio SMS API.</summary>
    Twilio,
    /// <summary>Vonage (formerly Nexmo) SMS API.</summary>
    Vonage,
    /// <summary>OVH SMS API.</summary>
    OVH,
    /// <summary>Azure Communication Services SMS API.</summary>
    Azure,
    /// <summary>Console logger provider for development.</summary>
    Console
}
