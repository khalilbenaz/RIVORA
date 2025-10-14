using RVR.Framework.Core.Modules;
using RVR.Framework.Notifications.Providers;
using RVR.Framework.Notifications.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RVR.Framework.Notifications;

public class NotificationsModule : IRvrModule
{
    public string Name => "Notifications";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IEmailService, SmtpEmailService>();
    }

    public void Configure(IApplicationBuilder app)
    {
    }
}
