using RVR.Framework.Export.Services;
using Microsoft.Extensions.DependencyInjection;

namespace RVR.Framework.Export.Extensions;

/// <summary>
/// Extension methods for registering export services in the dependency injection container.
/// </summary>
public static class ExportServiceCollectionExtensions
{
    /// <summary>
    /// Adds the RIVORA Framework export services (CSV, Excel, PDF) to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRvrExport(this IServiceCollection services)
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        services.AddScoped<IExportService, ExportService>();
        return services;
    }
}
