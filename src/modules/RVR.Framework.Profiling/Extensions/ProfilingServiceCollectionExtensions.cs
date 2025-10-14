namespace RVR.Framework.Profiling.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Profiling;

/// <summary>
/// Extension methods for registering MiniProfiler-based profiling services.
/// </summary>
public static class ProfilingServiceCollectionExtensions
{
    /// <summary>
    /// Adds RVR performance profiling (MiniProfiler) to the service collection.
    /// Profiling is enabled only in Development by default.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The hosting environment.</param>
    /// <param name="configure">Optional action to further configure <see cref="ProfilingOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRvrProfiling(
        this IServiceCollection services,
        IHostEnvironment? environment = null,
        Action<ProfilingOptions>? configure = null)
    {
        var profilingOptions = new ProfilingOptions();

        // Default: only enable in Development
        if (environment is not null)
        {
            profilingOptions.Enabled = environment.IsDevelopment();
        }

        configure?.Invoke(profilingOptions);

        services.AddSingleton(profilingOptions);

        if (!profilingOptions.Enabled)
        {
            return services;
        }

        // Configure MiniProfiler via the options pattern that MiniProfiler uses internally.
        services.Configure<MiniProfilerOptions>(miniProfilerOptions =>
        {
            miniProfilerOptions.RouteBasePath = profilingOptions.RouteBasePath;

            // Only profile in development-like environments
            miniProfilerOptions.ShouldProfile = _ => profilingOptions.Enabled;

            if (profilingOptions.SqlThresholdMs > 0)
            {
                miniProfilerOptions.TrackConnectionOpenClose = true;
            }
        });

        // Register MiniProfiler middleware dependencies.
        // The middleware itself is added via UseRvrProfiling().
        services.AddMemoryCache();

        return services;
    }
}
