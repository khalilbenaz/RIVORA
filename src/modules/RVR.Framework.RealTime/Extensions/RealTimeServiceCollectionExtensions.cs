using RVR.Framework.RealTime.Services;
using Microsoft.Extensions.DependencyInjection;

namespace RVR.Framework.RealTime.Extensions;

public static class RealTimeServiceCollectionExtensions
{
    public static IServiceCollection AddRvrRealTime(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddSingleton<INotificationService, SignalRNotificationService>();

        return services;
    }
}
