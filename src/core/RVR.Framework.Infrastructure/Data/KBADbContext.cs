using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using RVR.Framework.Core.Security;
using RVR.Framework.Domain;
using RVR.Framework.Domain.Entities.Auditing;
using RVR.Framework.Domain.Entities.BackgroundJobs;
using RVR.Framework.Domain.Entities.Configuration;
using RVR.Framework.Domain.Entities.Identity;
using RVR.Framework.Domain.Entities.MultiTenancy;
using RVR.Framework.Domain.Entities.Organization;
using RVR.Framework.Domain.Entities.Permissions;
using RVR.Framework.Domain.Entities.Products;
using RVR.Framework.Domain.Entities.Security;
using RVR.Framework.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace RVR.Framework.Infrastructure.Data;

/// <summary>
/// Contexte de base de données principal du framework RVR
/// </summary>
public class RVRDbContext : DbContext
{
    private readonly ITenantProvider? _tenantProvider;
    private readonly IEncryptionKeyProvider? _encryptionKeyProvider;

    /// <summary>
    /// Constructeur
    /// </summary>
    public RVRDbContext(
        DbContextOptions<RVRDbContext> options,
        ITenantProvider? tenantProvider = null,
        IEncryptionKeyProvider? encryptionKeyProvider = null) : base(options)
    {
        _tenantProvider = tenantProvider;
        _encryptionKeyProvider = encryptionKeyProvider;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = _tenantProvider?.GetConnectionString();
        if (!string.IsNullOrEmpty(connectionString))
        {
            optionsBuilder.UseSqlServer(connectionString);
        }

        base.OnConfiguring(optionsBuilder);
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
    public DbSet<RVR.Framework.Domain.Events.OutboxMessage> OutboxMessages => Set<RVR.Framework.Domain.Events.OutboxMessage>();

    // Security
    public DbSet<RVR.Framework.Domain.Entities.Security.ApiKey> ApiKeys => Set<RVR.Framework.Domain.Entities.Security.ApiKey>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Business Entities
    public DbSet<Product> Products => Set<Product>();

    [UnconditionalSuppressMessage("AOT", "IL2070:UnrecognizedReflectionPattern",
        Justification = "Domain entities with DomainEvents are known at compile time and tracked by EF Core.")]
    [UnconditionalSuppressMessage("AOT", "IL2075:UnrecognizedReflectionPattern",
        Justification = "Domain entities with DomainEvents/ClearDomainEvents are known at compile time and tracked by EF Core.")]
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Gestion du Soft Delete
        foreach (var entry in ChangeTracker.Entries<RVR.Framework.Domain.Entities.ISoftDelete>())
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
            var outboxMessages = domainEvents.Select(x => new RVR.Framework.Domain.Events.OutboxMessage
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
    [UnconditionalSuppressMessage("AOT", "IL2070:UnrecognizedReflectionPattern",
        Justification = "Attribute scanning for EncryptedAtRest occurs during EF Core model building which manages type metadata.")]
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuration globale du Soft Delete et du filtre de tenant
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var isSoftDelete = typeof(RVR.Framework.Domain.Entities.ISoftDelete).IsAssignableFrom(entityType.ClrType);
            var isTenantScoped = typeof(RVR.Framework.MultiTenancy.ITenantId).IsAssignableFrom(entityType.ClrType);

            if (isSoftDelete || isTenantScoped)
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                System.Linq.Expressions.Expression? filterBody = null;

                if (isSoftDelete)
                {
                    filterBody = System.Linq.Expressions.Expression.Equal(
                        System.Linq.Expressions.Expression.Property(parameter, nameof(RVR.Framework.Domain.Entities.ISoftDelete.IsDeleted)),
                        System.Linq.Expressions.Expression.Constant(false));
                }

                if (isTenantScoped && _tenantProvider != null)
                {
                    var tenantInfo = _tenantProvider.GetCurrentTenant();
                    if (tenantInfo != null && !string.IsNullOrEmpty(tenantInfo.Id))
                    {
                        var tenantFilter = System.Linq.Expressions.Expression.Equal(
                            System.Linq.Expressions.Expression.Property(parameter, nameof(RVR.Framework.MultiTenancy.ITenantId.TenantId)),
                            System.Linq.Expressions.Expression.Constant(tenantInfo.Id));
                        filterBody = filterBody != null
                            ? System.Linq.Expressions.Expression.AndAlso(filterBody, tenantFilter)
                            : tenantFilter;
                    }
                }

                if (filterBody != null)
                {
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(
                        System.Linq.Expressions.Expression.Lambda(filterBody, parameter));
                }
            }

            // Chiffrement des propriétés (EncryptedAtRest) via AES-256
            foreach (var property in entityType.GetProperties())
            {
                var attributes = property.PropertyInfo?.GetCustomAttributes(typeof(RVR.Framework.Core.Attributes.EncryptedAtRestAttribute), false);
                if (attributes != null && attributes.Any())
                {
                    var keyProvider = _encryptionKeyProvider
                        ?? throw new InvalidOperationException(
                            "An IEncryptionKeyProvider must be registered in the DI container when using " +
                            "[EncryptedAtRest] properties. Register EnvironmentEncryptionKeyProvider or " +
                            "ConfigurationEncryptionKeyProvider via AddEncryptionKeyProvider().");

                    var encryptionKey = keyProvider.GetEncryptionKey();

                    property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<string, string>(
                        v => EncryptString(v, encryptionKey),
                        v => DecryptString(v, encryptionKey)));
                }
            }
        }

        // Application des configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RVRDbContext).Assembly);
    }

    private static byte[] DeriveKey(string encryptionKey)
    {
        // Derive a unique salt from the encryption key to avoid shared static salt across instances
        var saltInput = $"RVR.Salt.{encryptionKey}";
        var salt = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(saltInput));
        return System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(
            encryptionKey,
            salt,
            iterations: 100_000,
            System.Security.Cryptography.HashAlgorithmName.SHA256,
            outputLength: 32);
    }

    private static string EncryptString(string plainText, string encryptionKey)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;
        using var aes = Aes.Create();
        aes.Key = DeriveKey(encryptionKey);
        aes.GenerateIV();
        using var encryptor = aes.CreateEncryptor();
        var plainBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        var result = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);
        return Convert.ToBase64String(result);
    }

    private static string DecryptString(string cipherText, string encryptionKey)
    {
        if (string.IsNullOrEmpty(cipherText)) return cipherText;
        var fullCipher = Convert.FromBase64String(cipherText);
        using var aes = Aes.Create();
        aes.Key = DeriveKey(encryptionKey);
        var iv = new byte[aes.BlockSize / 8];
        var cipher = new byte[fullCipher.Length - iv.Length];
        Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);
        aes.IV = iv;
        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
        return System.Text.Encoding.UTF8.GetString(plainBytes);
    }
}
