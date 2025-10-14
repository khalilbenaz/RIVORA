namespace RVR.Framework.ApiVersioning.Extensions;

using System;
using RVR.Framework.ApiVersioning.Conventions;
using RVR.Framework.ApiVersioning.Filters;
using RVR.Framework.ApiVersioning.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring RIVORA Framework API Versioning.
/// </summary>
public static class ApiVersioningServiceCollectionExtensions
{
    /// <summary>
    /// Adds RIVORA Framework API Versioning services with default configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRvrApiVersioning(this IServiceCollection services)
    {
        return AddRvrApiVersioning(services, _ => { });
    }

    /// <summary>
    /// Adds RIVORA Framework API Versioning services with custom configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure versioning options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRvrApiVersioning(
        this IServiceCollection services,
        Action<VersioningOptions> configureOptions)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddOptions<VersioningOptions>()
            .Configure(configureOptions);

        // Add ASP.NET Core API Versioning
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;

            // Configure version readers for URL path, header, and query string
            options.ApiVersionReader = Asp.Versioning.ApiVersionReader.Combine(
                new Asp.Versioning.UrlSegmentApiVersionReader(),
                new Asp.Versioning.HeaderApiVersionReader("X-API-Version"),
                new Asp.Versioning.QueryStringApiVersionReader("api-version"));
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    /// <summary>
    /// Adds API versioning with URL path strategy only.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure versioning options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRvrApiVersioningUrlPath(
        this IServiceCollection services,
        Action<VersioningOptions>? configureOptions = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddOptions<VersioningOptions>()
            .Configure(options =>
            {
                options.Strategies = VersioningStrategies.UrlPath;
                configureOptions?.Invoke(options);
            });

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    /// <summary>
    /// Adds API versioning with header strategy only.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure versioning options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRvrApiVersioningHeader(
        this IServiceCollection services,
        Action<VersioningOptions>? configureOptions = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddOptions<VersioningOptions>()
            .Configure(options =>
            {
                options.Strategies = VersioningStrategies.Header;
                configureOptions?.Invoke(options);
            });

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    /// <summary>
    /// Adds API versioning with query string strategy only.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure versioning options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRvrApiVersioningQueryString(
        this IServiceCollection services,
        Action<VersioningOptions>? configureOptions = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddOptions<VersioningOptions>()
            .Configure(options =>
            {
                options.Strategies = VersioningStrategies.QueryString;
                configureOptions?.Invoke(options);
            });

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    /// <summary>
    /// Adds API versioning with media type strategy.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="mediaTypeFormat">The media type format (e.g., "application/vnd.api.v{version}+json").</param>
    /// <param name="configureOptions">Action to configure versioning options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRvrApiVersioningMediaType(
        this IServiceCollection services,
        string mediaTypeFormat = "application/vnd.api.v{version}+json",
        Action<VersioningOptions>? configureOptions = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddOptions<VersioningOptions>()
            .Configure(options =>
            {
                options.Strategies = VersioningStrategies.MediaType;
                options.MediaTypeFormat = mediaTypeFormat;
                configureOptions?.Invoke(options);
            });

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    /// <summary>
    /// Configures API versioning conventions and filters.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddRvrApiVersioningConventions(this IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.Configure<MvcOptions>(options =>
        {
            // Add conventions
            options.Conventions.Add(new AutomaticVersioningConvention(
                services.BuildServiceProvider().GetRequiredService<Microsoft.Extensions.Options.IOptions<VersioningOptions>>()));

            // Add filters
            options.Filters.Add<ApiVersionResponseFilter>();
            options.Filters.Add<ApiVersionValidationFilter>();
            options.Filters.Add<SupportedVersionsFilter>();
        });

        return services;
    }
}

/// <summary>
/// Extension methods for API versioning middleware.
/// </summary>
public static class ApiVersioningApplicationBuilderExtensions
{
    /// <summary>
    /// Uses RIVORA Framework API Versioning middleware.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder.</returns>
    public static IApplicationBuilder UseRvrApiVersioning(this IApplicationBuilder app)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        // API Versioning is primarily configured through services
        // This method is for any additional middleware if needed

        return app;
    }

    /// <summary>
    /// Maps API versioning endpoints for documentation.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="path">The path for the versions endpoint.</param>
    /// <returns>The application builder.</returns>
    public static IApplicationBuilder MapRvrApiVersions(
        this IApplicationBuilder app,
        string path = "/api/versions")
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        app.Map(path, builder =>
        {
            builder.Run(async context =>
            {
                var options = context.RequestServices
                    .GetService<Microsoft.Extensions.Options.IOptions<VersioningOptions>>();

                if (options != null)
                {
                    context.Response.ContentType = "application/json";
                    var response = new
                    {
                        supportedVersions = options.Value.SupportedVersions,
                        defaultVersion = options.Value.DefaultVersion,
                        strategies = options.Value.Strategies.ToString(),
                        headerName = options.Value.HeaderName,
                        queryStringName = options.Value.QueryStringName,
                        urlPathPrefix = options.Value.UrlPathPrefix
                    };
                    await System.Text.Json.JsonSerializer.SerializeAsync(
                        context.Response.Body,
                        response);
                }
            });
        });

        return app;
    }
}
