namespace RVR.Framework.RealTime.Services;

/// <summary>
/// Service pour envoyer des notifications temps réel
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Envoie une notification à tous les utilisateurs connectés
    /// </summary>
    Task SendToAllAsync(string type, object data);

    /// <summary>
    /// Envoie une notification à un utilisateur spécifique
    /// </summary>
    Task SendToUserAsync(Guid userId, string type, object data);

    /// <summary>
    /// Envoie une notification à tous les utilisateurs d'un tenant
    /// </summary>
    Task SendToTenantAsync(Guid tenantId, string type, object data);

    /// <summary>
    /// Envoie une notification à un groupe SignalR spécifique
    /// </summary>
    Task SendToGroupAsync(string groupName, string type, object data);
}
