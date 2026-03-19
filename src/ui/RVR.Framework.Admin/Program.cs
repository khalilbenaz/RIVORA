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

// Database is optional — Studio features (wizard, modules, download) work without it.
// Admin features (dashboard, users, products, audit) require a database connection.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var hasDatabase = !string.IsNullOrEmpty(connectionString);

if (hasDatabase)
{
    builder.Services.AddDbContext<RVRDbContext>(options =>
    {
        options.UseSqlServer(connectionString!, sql =>
        {
            sql.MigrationsAssembly("RVR.Framework.Infrastructure");
        });
        options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    });
}
else
{
    // Register a dummy DbContext with in-memory provider so DI doesn't crash
    builder.Services.AddDbContext<RVRDbContext>(options =>
    {
        options.UseInMemoryDatabase("RvrStudioFallback");
    });
}

builder.Services.AddSingleton(new DatabaseStatus { IsConfigured = hasDatabase });

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

/// <summary>
/// Tracks whether a real database is configured.
/// </summary>
public class DatabaseStatus
{
    public bool IsConfigured { get; init; }
}
