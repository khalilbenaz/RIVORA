using RVR.Framework.Domain;
using RVR.Framework.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RVR.Framework.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité Role
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable(RVRConsts.TablePrefix + "Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(RVRConsts.MaxNameLength);

        builder.Property(r => r.NormalizedName)
            .IsRequired()
            .HasMaxLength(RVRConsts.MaxNameLength);

        builder.Property(r => r.Description)
            .HasMaxLength(RVRConsts.MaxDescriptionLength);

        builder.HasIndex(r => r.NormalizedName)
            .HasDatabaseName("IX_Roles_NormalizedName");

        builder.HasIndex(r => r.TenantId)
            .HasDatabaseName("IX_Roles_TenantId");

        // Query filter pour soft delete
        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
