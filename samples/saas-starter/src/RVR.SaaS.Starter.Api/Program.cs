using Serilog;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MediatR;
using Hangfire;
using RVR.SaaS.Starter.Infrastructure.Data;
using RVR.SaaS.Starter.Infrastructure.Repositories;
using RVR.SaaS.Starter.Identity;
using RVR.SaaS.Starter.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/saas-starter-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Database=saas_starter;Username=postgres;Password=postgres";
var identitySettings = builder.Configuration.GetSection("IdentitySettings").Get<IdentitySettings>() 
    ?? new IdentitySettings();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Identity
builder.Services.Configure<IdentitySettings>(builder.Configuration.GetSection("IdentitySettings"));
builder.Services.AddSingleton<IdentityService>();

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RVR.SaaS.Starter.Application.Commands.CreateTenantCommand).Assembly));

// Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(identitySettings.SecretKey)),
        ValidateIssuer = true,
        ValidIssuer = identitySettings.Issuer,
        ValidateAudience = true,
        ValidAudience = identitySettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Authorization
builder.Services.AddAuthorization();

// Hangfire
builder.Services.AddHangfire(config => config
    .UsePostgreSqlStorage(connectionString)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings());
builder.Services.AddHangfireServer();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.WithOrigins("http://localhost:5001", "https://localhost:5001")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "RVR SaaS Starter API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
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
});

var app = builder.Build();

// Middleware
app.UseSerilogRequestLogging();
app.UseCors("AllowBlazor");
app.UseAuthentication();
app.UseAuthorization();

// Hangfire Dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

// Background Jobs
RecurringJob.AddOrUpdate<EmailJob>("send-daily-summary", j => j.SendDailySummaryAsync(), Cron.Daily);

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await SeedDataAsync(db);
}

Log.Information("Starting RVR SaaS Starter API");
app.Run();

static async Task SeedDataAsync(AppDbContext db)
{
    if (!db.Tenants.Any())
    {
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Demo Tenant",
            Slug = "demo",
            PlanType = "Premium",
            SubscriptionStartDate = DateTime.UtcNow,
            IsActive = true
        };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        var admin = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@demo.com",
            PasswordHash = new IdentityService(Microsoft.Extensions.Options.Options.Create(new IdentitySettings())).HashPassword("Admin123!"),
            FirstName = "Admin",
            LastName = "User",
            Role = "Admin",
            TenantId = tenant.Id,
            EmailConfirmed = true
        };
        db.Users.Add(admin);
        await db.SaveChangesAsync();
    }
}

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true; // TODO: Add proper auth
}
