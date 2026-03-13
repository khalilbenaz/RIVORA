using KBA.Framework.Core.Modules;
using KBA.Framework.Notifications.Providers;
using KBA.Framework.Notifications.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KBA.Framework.Notifications;

public class NotificationsModule : IKbaModule
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
