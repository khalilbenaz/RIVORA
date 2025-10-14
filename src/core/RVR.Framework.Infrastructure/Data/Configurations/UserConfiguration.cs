using RVR.Framework.Domain;
using RVR.Framework.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RVR.Framework.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité User
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(RVRConsts.TablePrefix + "Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(RVRConsts.MaxNameLength);

        builder.Property(u => u.NormalizedUserName)
            .IsRequired()
            .HasMaxLength(RVRConsts.MaxNameLength);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(RVRConsts.MaxEmailLength);

        builder.Property(u => u.NormalizedEmail)
            .IsRequired()
            .HasMaxLength(RVRConsts.MaxEmailLength);

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(RVRConsts.MaxPhoneLength);

        builder.Property(u => u.FirstName)
            .HasMaxLength(RVRConsts.MaxNameLength);

        builder.Property(u => u.LastName)
            .HasMaxLength(RVRConsts.MaxNameLength);

        builder.HasIndex(u => u.NormalizedUserName)
            .HasDatabaseName("IX_Users_NormalizedUserName");

        builder.HasIndex(u => u.NormalizedEmail)
            .HasDatabaseName("IX_Users_NormalizedEmail");

        builder.HasIndex(u => u.TenantId)
            .HasDatabaseName("IX_Users_TenantId");

        // Query filter pour soft delete
        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}
