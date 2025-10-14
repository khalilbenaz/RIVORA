using Microsoft.EntityFrameworkCore;
using RVR.Framework.Infrastructure.Data;

namespace RVR.Framework.TestInfrastructure;

/// <summary>
/// Base class for test fixtures providing common setup including
/// an in-memory database context and a fake tenant provider.
/// </summary>
public abstract class TestFixtureBase : IDisposable
{
    /// <summary>
    /// The in-memory test database context.
    /// </summary>
    protected TestRVRDbContext DbContext { get; }

    /// <summary>
    /// The fake tenant provider for multi-tenancy scenarios.
    /// </summary>
    protected FakeTenantProvider TenantProvider { get; }

    protected TestFixtureBase()
    {
        TenantProvider = new FakeTenantProvider();
        DbContext = TestRVRDbContext.Create();
    }

    /// <summary>
    /// Saves all pending changes and detaches all tracked entities,
    /// useful for verifying data was persisted correctly.
    /// </summary>
    protected async Task SaveAndDetachAsync()
    {
        await DbContext.SaveChangesAsync();
        foreach (var entry in DbContext.ChangeTracker.Entries().ToList())
        {
            entry.State = EntityState.Detached;
        }
    }

    public virtual void Dispose()
    {
        DbContext.Database.EnsureDeleted();
        DbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}
