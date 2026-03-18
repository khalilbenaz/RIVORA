using RVR.Framework.SMS.Models;

namespace RVR.Framework.SMS;

/// <summary>
/// Provider abstraction for SMS delivery backends.
/// Each provider implementation communicates with a specific SMS gateway.
/// </summary>
public interface ISmsProvider
{
    /// <summary>
    /// Gets the provider type identifier.
    /// </summary>
    SmsProvider ProviderType { get; }

    /// <summary>
    /// Sends a single SMS message through this provider.
    /// </summary>
    /// <param name="message">The SMS message to send.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The result of the send operation.</returns>
    Task<SmsResult> SendAsync(SmsMessage message, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the delivery status of a previously sent message from this provider.
    /// </summary>
    /// <param name="messageId">The provider-assigned message identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The current delivery status.</returns>
    Task<SmsStatus> GetStatusAsync(string messageId, CancellationToken ct = default);
}
