namespace RVR.Framework.Alerting.Interfaces;

using RVR.Framework.Alerting.Models;

/// <summary>
/// Defines the contract for an alert notification channel.
/// </summary>
public interface IAlertChannel
{
    /// <summary>
    /// Sends an alert through this channel.
    /// </summary>
    /// <param name="alert">The alert to send.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendAsync(Alert alert, CancellationToken cancellationToken = default);
}
