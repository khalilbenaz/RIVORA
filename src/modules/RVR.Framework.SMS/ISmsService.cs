using RVR.Framework.SMS.Models;

namespace RVR.Framework.SMS;

/// <summary>
/// Core SMS service interface for sending messages and querying delivery status.
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// Sends a single SMS message through the configured provider.
    /// </summary>
    /// <param name="message">The SMS message to send.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The result of the send operation.</returns>
    Task<SmsResult> SendAsync(SmsMessage message, CancellationToken ct = default);

    /// <summary>
    /// Sends multiple SMS messages through the configured provider.
    /// </summary>
    /// <param name="messages">The SMS messages to send.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An aggregated result of the bulk send operation.</returns>
    Task<SmsResult> SendBulkAsync(IEnumerable<SmsMessage> messages, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the delivery status of a previously sent message.
    /// </summary>
    /// <param name="messageId">The provider-assigned message identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The current delivery status.</returns>
    Task<SmsStatus> GetStatusAsync(string messageId, CancellationToken ct = default);
}
