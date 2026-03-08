using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using KBA.Framework.Api.Middleware;
using KBA.Framework.Application.Services;
using KBA.Framework.Domain.Repositories;
using KBA.Framework.Infrastructure.Extensions;
using KBA.Framework.Infrastructure.Repositories;
using KBA.Framework.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

// Configuration de Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build())
    .CreateLogger();

try
{
    Log.Information("Démarrage de l'application KBA Framework");

    var builder = WebApplication.CreateBuilder(args);

    // Utiliser Serilog
    builder.Host.UseSerilog();

    // Configuration d'OpenTelemetry pour le Tracing et les Metrics
    var otlpEndpoint = builder.Configuration["OpenTelemetry:ExporterEndpoint"] ?? "http://localhost:4317";
    var serviceName = builder.Configuration["OpenTelemetry:ServiceName"] ?? "kba-framework";
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

    // Enregistrement des repositories
    builder.Services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
    builder.Services.AddScoped<IProductRepository, ProductRepository>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();

    // Enregistrement du contexte utilisateur
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();

    // Enregistrement des services
    builder.Services.AddScoped<IProductService, ProductService>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<JwtTokenService>();

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
        options.RequireHttpsMetadata = false; // En production, mettre à true
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
            Title = "KBA Framework API",
            Version = "v1",
            Description = "API du framework KBA - Clean Architecture avec multi-tenancy, sécurité JWT et validation\n\n" +
                         "## Comment utiliser l'API\n\n" +
                         "1. **Initialisation** : Si c'est la première utilisation, créez le premier admin via `/api/init/first-admin`\n" +
                         "2. **Authentification** : Utilisez `/api/auth/login` pour obtenir un token JWT\n" +
                         "3. **Autorisation** : Cliquez sur le bouton 'Authorize' et entrez votre token\n" +
                         "4. **Test** : Vous pouvez maintenant tester tous les endpoints protégés\n",
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Name = "KBA Framework",
                Email = "contact@kba-framework.com"
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
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    // Health checks
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

    var app = builder.Build();

    // Middleware de gestion globale des erreurs
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "KBA Framework API v1");
            c.DocumentTitle = "KBA Framework API - Swagger UI";
            c.DisplayRequestDuration();
            c.EnableTryItOutByDefault();
        });
        app.UseReDoc(options =>
        {
            options.SpecUrl = "/swagger/v1/swagger.json";
            options.DocumentTitle = "KBA Framework API Documentation";
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

    // Endpoints de santé et métriques
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => false // Liveness check basique
    });
    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Name != "self" // Readiness check avec dépendances
    });

    // Exposition des métriques Prometheus

    // Utiliser Serilog pour les requêtes HTTP
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? "unknown");
            diagnosticContext.Set("RequestId", httpContext.TraceIdentifier);
        };
    });
    
    app.UseHttpsRedirection();
    app.UseCors("AllowAll");
    
    // Servir les fichiers statiques (pour la page d'accueil)
    app.UseStaticFiles();
    app.UseDefaultFiles();
    
    // Authentification et autorisation
    app.UseAuthentication();
    app.UseAuthorization();
    
    app.MapControllers();

    Log.Information("Application KBA Framework démarrée avec succès");
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
