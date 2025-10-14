namespace RVR.Framework.Profiling.Extensions;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for adding profiling middleware to the request pipeline.
/// </summary>
public static class ProfilingApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the MiniProfiler middleware to the request pipeline.
    /// This should be called early in the pipeline, before MVC/routing middleware.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseRvrProfiling(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetService<ProfilingOptions>();

        if (options is null || !options.Enabled)
        {
            return app;
        }

        app.UseMiniProfiler();

        return app;
    }
}
