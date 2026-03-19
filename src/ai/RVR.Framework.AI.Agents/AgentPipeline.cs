using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RVR.Framework.AI.Agents.Execution;

namespace RVR.Framework.AI.Agents;

/// <summary>
/// Entry point for creating agent pipelines using a fluent API.
/// </summary>
public static class AgentPipeline
{
    /// <summary>
    /// Creates a new <see cref="AgentPipelineBuilder"/> for configuring an agent pipeline.
    /// </summary>
    /// <returns>A new pipeline builder instance.</returns>
    public static AgentPipelineBuilder Create() => new();
}

/// <summary>
/// Fluent builder for constructing an <see cref="IAgentPipeline"/>.
/// </summary>
public sealed class AgentPipelineBuilder
{
    private readonly List<Func<IServiceProvider, IAgent>> _agentFactories = [];
    private ExecutionStrategy _strategy = ExecutionStrategy.Sequential;
    private int _maxIterations = 10;
    private TimeSpan _timeout = TimeSpan.FromSeconds(120);

    /// <summary>
    /// Adds an agent instance to the pipeline.
    /// </summary>
    /// <param name="agent">The agent instance to add.</param>
    /// <returns>This builder for chaining.</returns>
    public AgentPipelineBuilder AddAgent(IAgent agent)
    {
        ArgumentNullException.ThrowIfNull(agent);
        _agentFactories.Add(_ => agent);
        return this;
    }

    /// <summary>
    /// Adds an agent resolved from the service provider to the pipeline.
    /// </summary>
    /// <typeparam name="TAgent">The agent type to resolve.</typeparam>
    /// <returns>This builder for chaining.</returns>
    public AgentPipelineBuilder AddAgent<TAgent>() where TAgent : IAgent
    {
        _agentFactories.Add(sp => sp.GetRequiredService<TAgent>());
        return this;
    }

    /// <summary>
    /// Sets the execution strategy for the pipeline.
    /// </summary>
    /// <param name="strategy">The strategy to use when running agents.</param>
    /// <returns>This builder for chaining.</returns>
    public AgentPipelineBuilder WithStrategy(ExecutionStrategy strategy)
    {
        _strategy = strategy;
        return this;
    }

    /// <summary>
    /// Sets the maximum number of iterations for iterative strategies.
    /// </summary>
    /// <param name="max">The maximum number of iterations.</param>
    /// <returns>This builder for chaining.</returns>
    public AgentPipelineBuilder WithMaxIterations(int max)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(max, 1);
        _maxIterations = max;
        return this;
    }

    /// <summary>
    /// Sets the overall timeout for pipeline execution.
    /// </summary>
    /// <param name="timeout">The maximum duration before the pipeline is cancelled.</param>
    /// <returns>This builder for chaining.</returns>
    public AgentPipelineBuilder WithTimeout(TimeSpan timeout)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeout, TimeSpan.Zero);
        _timeout = timeout;
        return this;
    }

    /// <summary>
    /// Builds the configured pipeline, resolving agents from the given service provider.
    /// </summary>
    /// <param name="provider">The service provider used to resolve agent dependencies.</param>
    /// <returns>A ready-to-execute agent pipeline.</returns>
    public IAgentPipeline Build(IServiceProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        var agents = _agentFactories.Select(f => f(provider)).ToList();
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger<IAgentPipeline>();

        return _strategy switch
        {
            ExecutionStrategy.Parallel => new ParallelPipelineExecutor(agents, _maxIterations, _timeout, logger),
            _ => new SequentialPipelineExecutor(agents, _maxIterations, _timeout, logger),
        };
    }
}

/// <summary>
/// Defines how agents in a pipeline are executed.
/// </summary>
public enum ExecutionStrategy
{
    /// <summary>
    /// Agents are executed one after another, each receiving the previous agent's output.
    /// </summary>
    Sequential,

    /// <summary>
    /// Agents are executed concurrently, and their results are aggregated.
    /// </summary>
    Parallel
}

/// <summary>
/// Represents an executable agent pipeline.
/// </summary>
public interface IAgentPipeline
{
    /// <summary>
    /// Executes the pipeline with the given input.
    /// </summary>
    /// <param name="input">The initial input text.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>The aggregated response from the pipeline.</returns>
    Task<AgentResponse> ExecuteAsync(string input, CancellationToken ct = default);
}
