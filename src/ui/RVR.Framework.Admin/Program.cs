using RVR.Framework.Admin.Components;
using RVR.Framework.Admin.Endpoints;
using RVR.Framework.Infrastructure.Data;
using RVR.Framework.MultiTenancy;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddHttpClient();

// Register a no-op tenant provider for the admin dashboard
builder.Services.AddSingleton<ITenantProvider, AdminTenantProvider>();

// Register the RVRDbContext with connection string from configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=RVRFrameworkDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true";

builder.Services.AddDbContext<RVRDbContext>(options =>
{
    options.UseSqlServer(connectionString, sql =>
    {
        sql.MigrationsAssembly("RVR.Framework.Infrastructure");
    });
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();
app.MapStudioEndpoints();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();

/// <summary>
/// Tenant provider for the admin dashboard (host-level, no tenant context)
/// </summary>
internal sealed class AdminTenantProvider : ITenantProvider
{
    public TenantInfo? GetCurrentTenant() => null;
    public string? GetConnectionString() => null;
}
