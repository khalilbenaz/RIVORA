namespace RVR.Framework.Alerting.Interfaces;

using RVR.Framework.Alerting.Models;

/// <summary>
/// Defines the contract for sending alerts.
/// </summary>
public interface IAlertService
{
    /// <summary>
    /// Sends an alert through all registered channels.
    /// </summary>
    /// <param name="alert">The alert to send.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendAlertAsync(Alert alert, CancellationToken cancellationToken = default);
}
