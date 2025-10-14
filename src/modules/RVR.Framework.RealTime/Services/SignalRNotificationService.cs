using RVR.Framework.RealTime.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace RVR.Framework.RealTime.Services;

/// <summary>
/// Implémentation du service de notification utilisant SignalR
/// </summary>
public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<KbaHub, IKbaHub> _hubContext;

    public SignalRNotificationService(IHubContext<KbaHub, IKbaHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendToAllAsync(string type, object data)
    {
        await _hubContext.Clients.All.ReceiveNotification(type, data);
    }

    public async Task SendToUserAsync(Guid userId, string type, object data)
    {
        await _hubContext.Clients.Group($"User_{userId}").ReceiveNotification(type, data);
    }

    public async Task SendToTenantAsync(Guid tenantId, string type, object data)
    {
        await _hubContext.Clients.Group(tenantId.ToString()).ReceiveNotification(type, data);
    }

    public async Task SendToGroupAsync(string groupName, string type, object data)
    {
        await _hubContext.Clients.Group(groupName).ReceiveNotification(type, data);
    }
}
