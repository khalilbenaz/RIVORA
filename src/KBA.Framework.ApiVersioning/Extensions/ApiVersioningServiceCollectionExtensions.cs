namespace KBA.Framework.ApiVersioning.Extensions;

using System;
using KBA.Framework.ApiVersioning.Conventions;
using KBA.Framework.ApiVersioning.Filters;
using KBA.Framework.ApiVersioning.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring KBA Framework API Versioning.
/// </summary>
public static class ApiVersioningServiceCollectionExtensions
{
    /// <summary>
    /// Adds KBA Framework API Versioning services with default configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddKbaApiVersioning(this IServiceCollection services)
    {
        return AddKbaApiVersioning(services, _ => { });
    }

    /// <summary>
    /// Adds KBA Framework API Versioning services with custom configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure versioning options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddKbaApiVersioning(
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

            // URL Path versioning (e.g., /v1/products)
            options.UrlPathSegmentName = "version";

            // Header versioning
            options.HeaderName = "X-API-Version";

            // Query string versioning
            options.QueryParameterName = "api-version";

            // Error response for unsupported versions
            options.ErrorResponseWriter = (context, error) =>
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                var response = new
                {
                    error = "Unsupported API version",
                    message = error.Message,
                    supportedVersions = error.SupportedVersions?.Select(v => v.ToString())
                };
                return System.Text.Json.JsonSerializer.SerializeAsync(
                    context.Response.Body,
                    response);
            };
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
    public static IServiceCollection AddKbaApiVersioningUrlPath(
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
    public static IServiceCollection AddKbaApiVersioningHeader(
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
    public static IServiceCollection AddKbaApiVersioningQueryString(
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
    public static IServiceCollection AddKbaApiVersioningMediaType(
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
    public static IServiceCollection AddKbaApiVersioningConventions(this IServiceCollection services)
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
    /// Uses KBA Framework API Versioning middleware.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder.</returns>
    public static IApplicationBuilder UseKbaApiVersioning(this IApplicationBuilder app)
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
    public static IApplicationBuilder MapKbaApiVersions(
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
