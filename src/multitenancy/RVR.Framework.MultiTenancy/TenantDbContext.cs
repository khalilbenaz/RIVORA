namespace RVR.Framework.MultiTenancy;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Base class for DbContext that automatically handles multi-tenancy.
/// </summary>
public abstract class TenantDbContext : DbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    protected TenantDbContext(DbContextOptions options, IHttpContextAccessor? httpContextAccessor = null) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets the current tenant ID from the HTTP context.
    /// </summary>
    protected string? CurrentTenantId => _httpContextAccessor?.HttpContext?.Items[TenantMiddleware.TenantKey] is TenantInfo info ? info.Id : null;

    public override int SaveChanges()
    {
        ApplyTenantFilter();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTenantFilter();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Applies the tenant ID to all entities being added that implement ITenantId.
    /// </summary>
    private void ApplyTenantFilter()
    {
        var tenantId = CurrentTenantId;
        if (string.IsNullOrEmpty(tenantId)) return;

        foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Added))
        {
            if (entry.Entity is ITenantId tenantEntity)
            {
                tenantEntity.TenantId = tenantId;
            }
        }
    }
}
