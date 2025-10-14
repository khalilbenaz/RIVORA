using Microsoft.EntityFrameworkCore;
using RVR.SaaS.Starter.Domain.Entities;

namespace RVR.SaaS.Starter.Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly Guid? _currentUserId;
    private readonly string? _tenantSchema;

    public AppDbContext(DbContextOptions<AppDbContext> options, Guid? currentUserId = null, string? tenantSchema = null)
        : base(options)
    {
        _currentUserId = currentUserId;
        _tenantSchema = tenantSchema;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply tenant schema if provided
        if (!string.IsNullOrEmpty(_tenantSchema))
        {
            modelBuilder.HasDefaultSchema(_tenantSchema);
        }

        // Configure entities
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasMany(e => e.Users).WithOne(e => e.Tenant).HasForeignKey(e => e.TenantId);
            entity.HasMany(e => e.Products).WithOne(e => e.Tenant).HasForeignKey(e => e.TenantId);
            entity.HasMany(e => e.Orders).WithOne(e => e.Tenant).HasForeignKey(e => e.TenantId);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasMany(e => e.AuditLogs).WithOne(e => e.User).HasForeignKey(e => e.UserId);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Sku);
            entity.HasMany(e => e.OrderItems).WithOne(e => e.Product).HasForeignKey(e => e.ProductId);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.HasMany(e => e.OrderItems).WithOne(e => e.Order).HasForeignKey(e => e.OrderId);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.OrderId, e.ProductId });
        });

        modelBuilder.Entity<FeatureFlag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.EntityName);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.CreatedBy = _currentUserId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedBy = _currentUserId;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
