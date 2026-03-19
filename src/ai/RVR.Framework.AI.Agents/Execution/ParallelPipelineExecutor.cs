using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace RVR.Framework.AI.Agents.Execution;

/// <summary>
/// Executes agents concurrently and aggregates their results.
/// </summary>
internal sealed class ParallelPipelineExecutor : IAgentPipeline
{
    private readonly IReadOnlyList<IAgent> _agents;
    private readonly int _maxIterations;
    private readonly TimeSpan _timeout;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="ParallelPipelineExecutor"/>.
    /// </summary>
    /// <param name="agents">The list of agents to execute in parallel.</param>
    /// <param name="maxIterations">The maximum number of pipeline iterations (unused for parallel, reserved for future use).</param>
    /// <param name="timeout">The overall timeout for pipeline execution.</param>
    /// <param name="logger">The logger instance.</param>
    internal ParallelPipelineExecutor(
        IReadOnlyList<IAgent> agents,
        int maxIterations,
        TimeSpan timeout,
        ILogger logger)
    {
        _agents = agents;
        _maxIterations = maxIterations;
        _timeout = timeout;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<AgentResponse> ExecuteAsync(string input, CancellationToken ct = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(_timeout);

        var context = new AgentContext(new Dictionary<string, object>(), []);
        var steps = new List<AgentStep>();
        var allSuccess = true;

        _logger.LogInformation("Starting parallel pipeline with {AgentCount} agent(s)", _agents.Count);

        var tasks = _agents.Select(async agent =>
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("Executing agent '{AgentName}' in parallel", agent.Name);

            try
            {
                var request = new AgentRequest(input, new AgentContext(new Dictionary<string, object>(), []));
                var response = await agent.ExecuteAsync(request, cts.Token).ConfigureAwait(false);
                stopwatch.Stop();

                _logger.LogDebug(
                    "Agent '{AgentName}' completed in {Duration}ms (success={Success})",
                    agent.Name, stopwatch.ElapsedMilliseconds, response.Success);

                return (agent.Name, response, stopwatch.Elapsed, Exception: (Exception?)null);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Agent '{AgentName}' threw an exception during parallel execution", agent.Name);

                var errorResponse = new AgentResponse(
                    ex.Message,
                    false,
                    new AgentContext(new Dictionary<string, object>(), []),
                    []);

                return (agent.Name, Response: errorResponse, stopwatch.Elapsed, Exception: ex);
            }
        }).ToList();

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);

        var aggregatedOutput = new StringBuilder();
        var mergedMemory = new Dictionary<string, object>();

        foreach (var (agentName, response, duration, exception) in results)
        {
            var step = new AgentStep(agentName, input, response.Output, duration, response.Success);
            steps.Add(step);
            steps.AddRange(response.Steps);

            if (!response.Success)
            {
                allSuccess = false;
            }

            // Merge memory from each agent's context
            foreach (var kvp in response.Context.Memory)
            {
                mergedMemory[kvp.Key] = kvp.Value;
            }

            if (aggregatedOutput.Length > 0)
            {
                aggregatedOutput.AppendLine();
                aggregatedOutput.AppendLine("---");
                aggregatedOutput.AppendLine();
            }

            aggregatedOutput.AppendLine($"[{agentName}]");
            aggregatedOutput.Append(response.Output);
        }

        var finalContext = new AgentContext(mergedMemory, [input]);
        _logger.LogInformation(
            "Parallel pipeline completed with {StepCount} step(s), success={Success}",
            steps.Count, allSuccess);

        return new AgentResponse(aggregatedOutput.ToString(), allSuccess, finalContext, steps);
    }
}
