using RVR.Framework.Domain;
using RVR.Framework.Domain.Entities.Permissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RVR.Framework.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité PermissionGrant
/// </summary>
public class PermissionGrantConfiguration : IEntityTypeConfiguration<PermissionGrant>
{
    public void Configure(EntityTypeBuilder<PermissionGrant> builder)
    {
        builder.ToTable(RVRConsts.TablePrefix + "PermissionGrants");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(RVRConsts.MaxNameLength);

        builder.Property(p => p.ProviderName)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(p => p.ProviderKey)
            .IsRequired()
            .HasMaxLength(128);

        builder.HasIndex(p => new { p.TenantId, p.Name, p.ProviderName, p.ProviderKey })
            .IsUnique()
            .HasDatabaseName("IX_PermissionGrants_Unique");
    }
}
