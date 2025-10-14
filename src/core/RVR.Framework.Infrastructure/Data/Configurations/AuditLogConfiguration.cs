using RVR.Framework.Domain;
using RVR.Framework.Domain.Entities.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RVR.Framework.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité AuditLog
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable(RVRConsts.TablePrefix + "AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.IpAddress)
            .HasMaxLength(64);

        builder.Property(a => a.BrowserInfo)
            .HasMaxLength(512);

        builder.Property(a => a.HttpMethod)
            .HasMaxLength(16);

        builder.Property(a => a.Url)
            .HasMaxLength(1024);

        builder.HasIndex(a => a.TenantId)
            .HasDatabaseName("IX_AuditLogs_TenantId");

        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("IX_AuditLogs_UserId");

        builder.HasIndex(a => a.ExecutionDate)
            .HasDatabaseName("IX_AuditLogs_ExecutionDate");
    }
}
