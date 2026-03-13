using KBA.Framework.Domain;
using KBA.Framework.Domain.Entities.Auditing;
using KBA.Framework.Domain.Entities.BackgroundJobs;
using KBA.Framework.Domain.Entities.Configuration;
using KBA.Framework.Domain.Entities.Identity;
using KBA.Framework.Domain.Entities.MultiTenancy;
using KBA.Framework.Domain.Entities.Organization;
using KBA.Framework.Domain.Entities.Permissions;
using KBA.Framework.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;

namespace KBA.Framework.Infrastructure.Data;

/// <summary>
/// Contexte de base de données principal du framework KBA
/// </summary>
public class KBADbContext : DbContext
{
    /// <summary>
    /// Constructeur
    /// </summary>
    public KBADbContext(DbContextOptions<KBADbContext> options) : base(options)
    {
    }

    // Identity
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<UserClaim> UserClaims => Set<UserClaim>();
    public DbSet<RoleClaim> RoleClaims => Set<RoleClaim>();
    public DbSet<UserLogin> UserLogins => Set<UserLogin>();
    public DbSet<UserToken> UserTokens => Set<UserToken>();

    // Multi-Tenancy
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantConnectionString> TenantConnectionStrings => Set<TenantConnectionString>();

    // Permissions
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<PermissionGrant> PermissionGrants => Set<PermissionGrant>();

    // Audit Logging
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AuditLogAction> AuditLogActions => Set<AuditLogAction>();
    public DbSet<EntityChange> EntityChanges => Set<EntityChange>();
    public DbSet<EntityPropertyChange> EntityPropertyChanges => Set<EntityPropertyChange>();

    // Configuration
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<FeatureValue> FeatureValues => Set<FeatureValue>();

    // Organization
    public DbSet<OrganizationUnit> OrganizationUnits => Set<OrganizationUnit>();
    public DbSet<UserOrganizationUnit> UserOrganizationUnits => Set<UserOrganizationUnit>();

    // Background Jobs
    public DbSet<BackgroundJob> BackgroundJobs => Set<BackgroundJob>();

    // Outbox
    public DbSet<KBA.Framework.Domain.Events.OutboxMessage> OutboxMessages => Set<KBA.Framework.Domain.Events.OutboxMessage>();

    // Security
    public DbSet<KBA.Framework.Domain.Entities.Security.ApiKey> ApiKeys => Set<KBA.Framework.Domain.Entities.Security.ApiKey>();

    // Business Entities
    public DbSet<Product> Products => Set<Product>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Gestion du Soft Delete
        foreach (var entry in ChangeTracker.Entries<KBA.Framework.Domain.Entities.ISoftDelete>())
        {
            switch (entry.State)
            {
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAtUtc = DateTime.UtcNow;
                    break;
            }
        }

        // Capture domain events from tracked entities
        var domainEvents = ChangeTracker.Entries()
            .Where(x => x.Entity.GetType().GetProperty("DomainEvents") != null)
            .Select(x => (Entity: x.Entity, Events: (IEnumerable<object>)x.Entity.GetType().GetProperty("DomainEvents")!.GetValue(x.Entity)!))
            .SelectMany(x => x.Events.Select(e => (x.Entity, Event: e)))
            .ToList();

        if (domainEvents.Any())
        {
            var outboxMessages = domainEvents.Select(x => new KBA.Framework.Domain.Events.OutboxMessage
            {
                Type = x.Event.GetType().FullName ?? x.Event.GetType().Name,
                Content = System.Text.Json.JsonSerializer.Serialize(x.Event),
                OccurredOnUtc = DateTime.UtcNow
            }).ToList();

            await OutboxMessages.AddRangeAsync(outboxMessages, cancellationToken);
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        // Clear events after successful save
        foreach (var entry in domainEvents)
        {
            entry.Entity.GetType().GetMethod("ClearDomainEvents")?.Invoke(entry.Entity, null);
        }

        return result;
    }

    /// <summary>
    /// Configuration du modèle
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuration globale du Soft Delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(KBA.Framework.Domain.Entities.ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var body = System.Linq.Expressions.Expression.Equal(
                    System.Linq.Expressions.Expression.Property(parameter, nameof(KBA.Framework.Domain.Entities.ISoftDelete.IsDeleted)),
                    System.Linq.Expressions.Expression.Constant(false));
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(System.Linq.Expressions.Expression.Lambda(body, parameter));
            }

            // Chiffrement des propriétés (EncryptedAtRest)
            foreach (var property in entityType.GetProperties())
            {
                var attributes = property.PropertyInfo?.GetCustomAttributes(typeof(KBA.Framework.Core.Attributes.EncryptedAtRestAttribute), false);
                if (attributes != null && attributes.Any())
                {
                    // Note: En production, utilisez un vrai fournisseur de chiffrement et une clé sécurisée (Key Vault)
                    property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<string, string>(
                        v => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(v)), // "Chiffrement" basique pour l'exemple
                        v => System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(v))));
                }
            }
        }

        // Application des configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KBADbContext).Assembly);
    }
}
