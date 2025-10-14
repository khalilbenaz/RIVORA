using RVR.Framework.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace RVR.Framework.TestInfrastructure;

/// <summary>
/// In-memory database context for unit testing.
/// Uses the EF Core InMemory provider so no real database is required.
/// </summary>
public class TestRVRDbContext : RVRDbContext
{
    public TestRVRDbContext()
        : base(CreateOptions())
    {
    }

    public TestRVRDbContext(DbContextOptions<RVRDbContext> options)
        : base(options)
    {
    }

    private static DbContextOptions<RVRDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<RVRDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
    }

    /// <summary>
    /// Creates a new test context with a unique in-memory database.
    /// </summary>
    public static TestRVRDbContext Create()
    {
        var context = new TestRVRDbContext();
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    /// Creates a new test context using the specified database name (useful for sharing state across tests).
    /// </summary>
    public static TestRVRDbContext Create(string databaseName)
    {
        var options = new DbContextOptionsBuilder<RVRDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;
        var context = new TestRVRDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
