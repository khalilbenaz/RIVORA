// Entry point for Linux / non-MAUI builds (Blazor Server standalone).
// On Windows/macOS, MauiProgram.cs is the entry point via MAUI Blazor Hybrid.

using RVR.Framework.Admin.Components;
using RVR.Framework.Admin.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddHttpClient();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();
app.MapStudioEndpoints();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

Console.WriteLine("RVR Studio running at http://localhost:5200");
app.Run("http://localhost:5200");
