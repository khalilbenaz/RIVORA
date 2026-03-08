using Serilog;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MassTransit;
using KBA.Microservices.Shared.Domain.Entities;
using KBA.Microservices.Shared.Application.Events;
using KBA.Microservices.Shared.Infrastructure.Messaging;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=postgres_identity;Database=identity_db;Username=postgres;Password=postgres";

// Database
builder.Services.AddDbContext<IdentityDbContext>(options => options.UseNpgsql(connectionString));

// JWT
var jwtKey = builder.Configuration["JwtSettings:SecretKey"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "KBA.Microservices",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JwtSettings:Audience"] ?? "KBA.Microservices.Users",
            ValidateLifetime = true
        };
    });

// gRPC
builder.Services.AddGrpc();

// RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });
        cfg.ConfigureEndpoints(context);
    });
});
builder.Services.AddScoped<IEventBus, MassTransitEventBus>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<IdentityGrpcService>();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await db.Database.MigrateAsync();
    await SeedDataAsync(db);
}

Log.Information("Starting Identity Service");
app.Run();

static async Task SeedDataAsync(IdentityDbContext db)
{
    if (!db.Users.Any())
    {
        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@kba.com",
            PasswordHash = "admin123", // TODO: Hash properly
            FirstName = "Admin",
            LastName = "User",
            Role = "Admin",
            IsActive = true
        });
        await db.SaveChangesAsync();
    }
}

public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }
    public DbSet<User> Users => Set<User>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }
}

public class IdentityGrpcService : IdentityService.IdentityServiceBase
{
    private readonly IdentityDbContext _db;
    private readonly ILogger<IdentityGrpcService> _logger;

    public IdentityGrpcService(IdentityDbContext db, ILogger<IdentityGrpcService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public override Task<ValidateTokenResponse> ValidateToken(ValidateTokenRequest request, ServerCallContext context)
    {
        // TODO: Implement actual token validation
        return Task.FromResult(new ValidateTokenResponse
        {
            IsValid = !string.IsNullOrEmpty(request.Token),
            UserId = Guid.NewGuid().ToString(),
            Email = "user@example.com",
            Role = "User"
        });
    }

    public override async Task<GetUserByIdResponse> GetUserById(GetUserByIdRequest request, ServerCallContext context)
    {
        var user = await _db.Users.FindAsync(Guid.Parse(request.UserId));
        if (user == null)
            return new GetUserByIdResponse();

        return new GetUserByIdResponse
        {
            UserId = user.Id.ToString(),
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role
        };
    }
}
