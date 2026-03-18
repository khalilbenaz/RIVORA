namespace RVR.Framework.Core.Seeding;

/// <summary>
/// Defines a standardized data seeder for RIVORA projects.
/// Implement this interface to create seeders that can be discovered and executed by <c>rvr seed</c>.
/// </summary>
public interface IRvrDataSeeder
{
    /// <summary>
    /// Gets the seeding profile this seeder belongs to (e.g., "dev", "demo", "test", "perf").
    /// </summary>
    string Profile { get; }

    /// <summary>
    /// Gets the execution order. Lower values run first.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Executes the seeding logic.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SeedAsync(CancellationToken cancellationToken = default);
}
