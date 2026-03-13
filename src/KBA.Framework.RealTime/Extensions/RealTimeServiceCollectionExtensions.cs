using KBA.Framework.RealTime.Services;
using Microsoft.Extensions.DependencyInjection;

namespace KBA.Framework.RealTime.Extensions;

public static class RealTimeServiceCollectionExtensions
{
    public static IServiceCollection AddKbaRealTime(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddSingleton<INotificationService, SignalRNotificationService>();
        
        return services;
    }
}
