using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using RVR.Framework.Api.Middleware;
using RVR.Framework.Application.Services;
using RVR.Framework.Domain.Repositories;
using RVR.Framework.Infrastructure.Extensions;
using RVR.Framework.Infrastructure.Repositories;
using RVR.Framework.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using RVR.Framework.Core.Modules;
using RVR.Framework.RealTime.Hubs;
using Microsoft.AspNetCore.RateLimiting;
using RVR.Framework.Api.Extensions;
using RVR.Framework.Security.Extensions;
using RVR.Framework.Security.Interfaces;
using RVR.Framework.Security.Services;

// Configuration de Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build())
    .CreateLogger();

try
{
    Log.Information("Démarrage de l'application RIVORA Framework");

    var builder = WebApplication.CreateBuilder(args);

    // Aspire Service Defaults (7.5)
    builder.AddServiceDefaults();

    // Utiliser Serilog
    builder.Host.UseSerilog();

    // Configuration d'OpenTelemetry pour le Tracing et les Metrics
    var otlpEndpoint = builder.Configuration["OpenTelemetry:ExporterEndpoint"] ?? "http://localhost:4317";
    var serviceName = builder.Configuration["OpenTelemetry:ServiceName"] ?? "RIVORA-framework";
    var serviceVersion = builder.Configuration["OpenTelemetry:ServiceVersion"] ?? "1.0.0";

    // Configuration du Tracing
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource
            .AddService(serviceName: serviceName,
                       serviceVersion: serviceVersion,
                       serviceInstanceId: Environment.MachineName)
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment"] = builder.Environment.EnvironmentName,
                ["host.name"] = Environment.MachineName,
                ["os.type"] = Environment.OSVersion.Platform.ToString()
            }))
        .WithTracing(tracing =>
        {
            tracing
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.EnrichWithHttpRequest = (activity, request) =>
                    {
                        activity.SetTag("request.route", request.Path);
                        activity.SetTag("request.method", request.Method);
                    };
                    options.EnrichWithHttpResponse = (activity, response) =>
                    {
                        activity.SetTag("response.status", response.StatusCode);
                    };
                    options.RecordException = true;
                    options.Filter = (context) =>
                    {
                        // Exclure les endpoints de santé et de métriques
                        var path = context.Request.Path;
                        return !path.StartsWithSegments("/health") &&
                               !path.StartsWithSegments("/metrics") &&
                               !path.StartsWithSegments("/swagger");
                    };
                })
                .AddHttpClientInstrumentation(options =>
                {
                    options.RecordException = true;
                })

                .AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri(otlpEndpoint);
                    otlpOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                });
        })
        .WithMetrics(metrics =>
        {
            metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri(otlpEndpoint);
                    otlpOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                });
        });

    // Configuration de la base de données optimisée
    builder.Services.AddOptimizedDbContext(builder.Configuration);

    // Enregistrement des modules RVR (2.4)
    builder.Services.AddRvrModules(builder.Configuration,
        typeof(RVR.Framework.RealTime.RealTimeModule).Assembly,
        typeof(RVR.Framework.Notifications.NotificationsModule).Assembly);

    // Compression de réponse
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
    });

    // Output Caching (.NET 8+)
    builder.Services.AddOutputCache(options =>
    {
        options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromMinutes(5)));
        options.AddPolicy("CacheProductList", builder =>
            builder.Expire(TimeSpan.FromMinutes(10)).Tag("products"));
    });

    // Enregistrement des repositories
    builder.Services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
    builder.Services.AddScoped<IProductRepository, ProductRepository>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();

    // Enregistrement du contexte utilisateur
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();
    builder.Services.AddScoped<RVR.Framework.MultiTenancy.ITenantProvider, RVR.Framework.MultiTenancy.HttpTenantProvider>();

    // Configuration du verrouillage de compte (anti brute-force)
    builder.Services.Configure<RVR.Framework.Infrastructure.Configuration.AccountLockoutOptions>(
        builder.Configuration.GetSection(RVR.Framework.Infrastructure.Configuration.AccountLockoutOptions.SectionName));

    // Enregistrement des repositories (refresh tokens)
    builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

    // Configuration des refresh tokens
    builder.Services.Configure<RefreshTokenOptions>(options =>
    {
        var refreshDays = builder.Configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationDays", 7);
        options.ExpirationDays = refreshDays;
        options.EnableRotation = true;
        options.RevokeOnReuse = true;
        options.MaxActiveTokensPerUser = 10;
        options.RevokedTokenRetentionDays = 30;
    });

    // Enregistrement des services de refresh tokens (persistent store + service)
    builder.Services.AddScoped<IRefreshTokenStore, EfRefreshTokenStore>();
    builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

    // Enregistrement des services de securite (PasswordHasher, etc.)
    builder.Services.AddRvrSecurity();

    // Enregistrement des services
    builder.Services.AddScoped<IProductService, ProductService>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<JwtTokenService>();

    // Rate Limiting (4.3)
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.AddFixedWindowLimiter("fixed", opt =>
        {
            opt.Window = TimeSpan.FromSeconds(10);
            opt.PermitLimit = 100;
            opt.QueueLimit = 2;
            opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        });

        options.AddConcurrencyLimiter("concurrency", opt =>
        {
            opt.PermitLimit = 10;
            opt.QueueLimit = 5;
            opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        });

        // Strict rate limiting for sensitive endpoints (login, auth)
        options.AddFixedWindowLimiter("strict", opt =>
        {
            opt.Window = TimeSpan.FromMinutes(1);
            opt.PermitLimit = 5;
            opt.QueueLimit = 0;
            opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        });
    });

    // Localization
    builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

    // Discovery des Minimal APIs (7.4)
    var endpointMapperType = typeof(RVR.Framework.Api.Interfaces.IMapEndpoints);
    var endpointMappers = typeof(Program).Assembly.GetTypes()
        .Where(t => endpointMapperType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

    foreach (var mapper in endpointMappers)
    {
        builder.Services.AddScoped(endpointMapperType, mapper);
    }

    // gRPC (3.4)
    builder.Services.AddGrpc();

    // Controllers avec FluentValidation et découverte API
    builder.Services.AddControllers();

    // FluentValidation
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddFluentValidationClientsideAdapters();
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();
    builder.Services.AddValidatorsFromAssembly(typeof(IProductService).Assembly);

    // Authentication JWT
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey non configurée");

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

    builder.Services.AddAuthorization();

    // Swagger/OpenAPI avec support JWT
    builder.Services.AddEndpointsApiExplorer();

    // Configuration pour exposer tous les endpoints
    builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
    {
        options.SuppressMapClientErrors = false;
    });

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "RIVORA Framework API",
            Version = "v1",
            Description = "API du framework RVR - Clean Architecture avec multi-tenancy, sécurité JWT et validation\n\n" +
                         "## Comment utiliser l'API\n\n" +
                         "1. **Initialisation** : Si c'est la première utilisation, créez le premier admin via `/api/init/first-admin`\n" +
                         "2. **Authentification** : Utilisez `/api/auth/login` pour obtenir un token JWT\n" +
                         "3. **Autorisation** : Cliquez sur le bouton 'Authorize' et entrez votre token\n" +
                         "4. **Test** : Vous pouvez maintenant tester tous les endpoints protégés\n",
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Name = "RIVORA Framework",
                Email = "contact@RIVORA-framework.com"
            }
        });

        // Configuration JWT pour Swagger avec support Bearer Token
        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description = "Entrez le token JWT avec le préfixe Bearer. Exemple: 'Bearer eyJhbGciOiJIUzI1Ni...'\n\n" +
                         "Vous pouvez obtenir un token en utilisant l'endpoint /api/auth/login",
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

        options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        // Configurer l'inclusion de tous les endpoints
        options.DocInclusionPredicate((docName, apiDesc) => true);

        // Inclure les commentaires XML
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            if (builder.Environment.IsDevelopment())
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            }
            else
            {
                var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                    ?? Array.Empty<string>();
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            }
        });
    });

    // Health checks (already registered in ServiceDefaults, just add SQL Server)
    builder.Services.AddHealthChecks()
        .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!, name: "database", tags: new[] { "ready" });

    var app = builder.Build();

    // Middleware de sécurité OWASP
    app.UseMiddleware<SecurityHeadersMiddleware>();

    // Middleware de gestion globale des erreurs
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "RIVORA Framework API v1");
            c.DocumentTitle = "RIVORA Framework API - Swagger UI";
            c.DisplayRequestDuration();
            c.EnableTryItOutByDefault();
        });
        app.UseReDoc(options =>
        {
            options.SpecUrl = "/swagger/v1/swagger.json";
            options.DocumentTitle = "RIVORA Framework API Documentation";
            options.RoutePrefix = "api-docs";

            // Options ReDoc pour une meilleure interactivité
            options.ConfigObject = new Swashbuckle.AspNetCore.ReDoc.ConfigObject
            {
                HideDownloadButton = false,
                ExpandResponses = "200,201",
                RequiredPropsFirst = true,
                NoAutoAuth = false,
                PathInMiddlePanel = false,
                HideLoading = false,
                NativeScrollbars = false,
                DisableSearch = false,
                OnlyRequiredInSamples = false,
                SortPropsAlphabetically = true
            };
        });
    }

    // Endpoints de santé (définis dans ServiceDefaults: /health, /alive, /healthz, /ready)
    app.MapDefaultEndpoints();

    // Exposition des métriques Prometheus

    // Utiliser Serilog pour les requêtes HTTP
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms | Tenant: {TenantId} | User: {UserId}";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? "unknown");
            diagnosticContext.Set("RequestId", httpContext.TraceIdentifier);

            // Récupérer le contexte utilisateur pour l'enrichissement
            var userContext = httpContext.RequestServices.GetService<ICurrentUserContext>();
            if (userContext != null)
            {
                diagnosticContext.Set("TenantId", userContext.TenantId?.ToString() ?? "none");
                diagnosticContext.Set("UserId", userContext.UserId?.ToString() ?? "none");
                diagnosticContext.Set("UserName", userContext.UserName ?? "anonymous");
            }
        };
    });

    app.UseOutputCache();
    app.UseResponseCompression();
    app.UseMiddleware<ETagMiddleware>();
    app.UseHttpsRedirection();
    app.UseCors("AllowAll");
    app.UseRateLimiter();

    // Configuration de la localisation
    var supportedCultures = new[] { "fr-FR", "en-US" };
    var localizationOptions = new RequestLocalizationOptions()
        .SetDefaultCulture(supportedCultures[0])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);

    app.UseRequestLocalization(localizationOptions);

    // Servir les fichiers statiques (pour la page d'accueil)
    app.UseStaticFiles();
    app.UseDefaultFiles();

    // Authentification par clé API
    app.UseMiddleware<ApiKeyAuthenticationMiddleware>();

    // Authentification et autorisation
    app.UseAuthentication();
    app.UseAuthorization();

    // Configuration des modules RVR (2.4)
    app.UseRvrModules();

    app.MapControllers();
    app.MapRvrEndpoints();
    // app.MapGrpcService<KbaService>(); // À implémenter selon les besoins

    Log.Information("Application RIVORA Framework démarrée avec succès");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "L'application a échoué au démarrage");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

// Make the implicit Program class public for integration tests
public partial class Program { }
