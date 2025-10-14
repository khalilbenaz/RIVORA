using RVR.Framework.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace RVR.Framework.RealTime.Hubs;

/// <summary>
/// Hub de base pour RIVORA Framework avec support du multi-tenancy
/// </summary>
[Authorize]
public class KbaHub : Hub<IKbaHub>
{
    private readonly ICurrentUserContext _currentUserContext;

    public KbaHub(ICurrentUserContext currentUserContext)
    {
        _currentUserContext = currentUserContext;
    }

    public override async Task OnConnectedAsync()
    {
        if (_currentUserContext.TenantId.HasValue)
        {
            // Ajouter l'utilisateur au groupe de son tenant
            await Groups.AddToGroupAsync(Context.ConnectionId, _currentUserContext.TenantId.Value.ToString());
        }

        if (_currentUserContext.UserId.HasValue)
        {
            // Ajouter l'utilisateur à son propre groupe personnel
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{_currentUserContext.UserId.Value}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_currentUserContext.TenantId.HasValue)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, _currentUserContext.TenantId.Value.ToString());
        }

        await base.OnDisconnectedAsync(exception);
    }
}
