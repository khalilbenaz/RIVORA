using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace RVR.Framework.AI.Agents.Execution;

/// <summary>
/// Executes agents sequentially, passing the output of each agent as the input to the next.
/// </summary>
internal sealed class SequentialPipelineExecutor : IAgentPipeline
{
    private readonly IReadOnlyList<IAgent> _agents;
    private readonly int _maxIterations;
    private readonly TimeSpan _timeout;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="SequentialPipelineExecutor"/>.
    /// </summary>
    /// <param name="agents">The ordered list of agents to execute.</param>
    /// <param name="maxIterations">The maximum number of pipeline iterations.</param>
    /// <param name="timeout">The overall timeout for pipeline execution.</param>
    /// <param name="logger">The logger instance.</param>
    internal SequentialPipelineExecutor(
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
        var currentInput = input;
        var iteration = 0;

        _logger.LogInformation("Starting sequential pipeline with {AgentCount} agent(s)", _agents.Count);

        foreach (var agent in _agents)
        {
            if (iteration >= _maxIterations)
            {
                _logger.LogWarning("Maximum iterations ({Max}) reached, stopping pipeline", _maxIterations);
                break;
            }

            cts.Token.ThrowIfCancellationRequested();

            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("Executing agent '{AgentName}' with input length {Length}", agent.Name, currentInput.Length);

            try
            {
                var request = new AgentRequest(currentInput, context);
                var response = await agent.ExecuteAsync(request, cts.Token).ConfigureAwait(false);

                stopwatch.Stop();

                var step = new AgentStep(agent.Name, currentInput, response.Output, stopwatch.Elapsed, response.Success);
                steps.Add(step);
                steps.AddRange(response.Steps);

                context = response.Context;
                currentInput = response.Output;

                _logger.LogDebug(
                    "Agent '{AgentName}' completed in {Duration}ms (success={Success})",
                    agent.Name, stopwatch.ElapsedMilliseconds, response.Success);

                if (!response.Success)
                {
                    _logger.LogWarning("Agent '{AgentName}' failed, stopping pipeline", agent.Name);
                    return new AgentResponse(response.Output, false, context, steps);
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Agent '{AgentName}' threw an exception", agent.Name);

                steps.Add(new AgentStep(agent.Name, currentInput, ex.Message, stopwatch.Elapsed, false));
                return new AgentResponse(ex.Message, false, context, steps);
            }

            iteration++;
        }

        _logger.LogInformation("Sequential pipeline completed with {StepCount} step(s)", steps.Count);
        return new AgentResponse(currentInput, true, context, steps);
    }
}
