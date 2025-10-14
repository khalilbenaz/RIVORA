namespace RVR.Framework.TestInfrastructure.Builders;

/// <summary>
/// Base class for test entity builders implementing the builder pattern.
/// </summary>
/// <typeparam name="TEntity">The entity type being built.</typeparam>
/// <typeparam name="TBuilder">The concrete builder type (for fluent returns).</typeparam>
public abstract class TestEntityBuilder<TEntity, TBuilder>
    where TBuilder : TestEntityBuilder<TEntity, TBuilder>
{
    /// <summary>
    /// Builds the entity with the configured values.
    /// </summary>
    public abstract TEntity Build();

    /// <summary>
    /// Returns this builder as the concrete type for fluent chaining.
    /// </summary>
    protected TBuilder This => (TBuilder)this;
}
