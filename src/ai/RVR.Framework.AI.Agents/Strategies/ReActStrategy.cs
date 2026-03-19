using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using RVR.Framework.AI.Abstractions;

namespace RVR.Framework.AI.Agents.Strategies;

/// <summary>
/// An agent that implements the ReAct (Reason + Act) pattern. It iteratively reasons
/// about the task, selects and executes tools, observes the results, and repeats
/// until a final answer is produced or the iteration limit is reached.
/// </summary>
public sealed partial class ReActAgent : IAgent
{
    private readonly IChatClient _chatClient;
    private readonly IReadOnlyList<ITool> _tools;
    private readonly ILogger<ReActAgent> _logger;
    private readonly int _maxIterations;
    private readonly string _model;

    private const string SystemPrompt =
        """
        You are a reasoning agent. You solve problems by iterating through Thought/Action/Observation steps.

        Available tools:
        {tools}

        Respond EXACTLY in one of these formats:

        When you need to use a tool:
        Thought: <your reasoning about what to do next>
        Action: <tool_name>
        Action Input: <JSON parameters for the tool>

        When you have the final answer:
        Thought: <your final reasoning>
        Final Answer: <your complete answer>

        Always start with a Thought. Never skip the Thought step.
        """;

    /// <summary>
    /// Initializes a new instance of <see cref="ReActAgent"/>.
    /// </summary>
    /// <param name="chatClient">The LLM client used for reasoning.</param>
    /// <param name="tools">The tools available for the agent to use.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="maxIterations">The maximum number of Thought-Action-Observation loops.</param>
    /// <param name="model">The model identifier to use for LLM requests.</param>
    public ReActAgent(
        IChatClient chatClient,
        IEnumerable<ITool> tools,
        ILogger<ReActAgent> logger,
        int maxIterations = 10,
        string model = "gpt-4")
    {
        ArgumentNullException.ThrowIfNull(chatClient);
        ArgumentNullException.ThrowIfNull(tools);
        ArgumentNullException.ThrowIfNull(logger);

        _chatClient = chatClient;
        _tools = tools.ToList();
        _logger = logger;
        _maxIterations = maxIterations;
        _model = model;
    }

    /// <inheritdoc />
    public string Name => "ReAct";

    /// <inheritdoc />
    public string Description => "A reasoning agent that uses the ReAct (Reason + Act) pattern to iteratively solve problems using available tools.";

    /// <inheritdoc />
    public async Task<AgentResponse> ExecuteAsync(AgentRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var context = request.Context ?? new AgentContext(new Dictionary<string, object>(), []);
        var steps = new List<AgentStep>();
        var messages = new List<ChatMessage>();
        var toolDescriptions = BuildToolDescriptions();

        var systemPrompt = SystemPrompt.Replace("{tools}", toolDescriptions);
        messages.Add(new ChatMessage { Role = "user", Content = request.Input });

        _logger.LogInformation("Starting ReAct loop for input (length={Length})", request.Input.Length);

        for (var iteration = 0; iteration < _maxIterations; iteration++)
        {
            ct.ThrowIfCancellationRequested();

            var stopwatch = Stopwatch.StartNew();

            var chatRequest = new ChatRequest
            {
                Model = _model,
                SystemPrompt = systemPrompt,
                Messages = [.. messages],
                Temperature = 0.1f,
                MaxTokens = 2048
            };

            var chatResponse = await _chatClient.ChatAsync(chatRequest, ct).ConfigureAwait(false);
            var llmOutput = chatResponse.Content;

            _logger.LogDebug("ReAct iteration {Iteration}: LLM response length={Length}", iteration, llmOutput.Length);

            messages.Add(new ChatMessage { Role = "assistant", Content = llmOutput });

            // Check for final answer
            var finalAnswer = ExtractFinalAnswer(llmOutput);
            if (finalAnswer is not null)
            {
                stopwatch.Stop();
                steps.Add(new AgentStep(Name, request.Input, finalAnswer, stopwatch.Elapsed, true));
                context = context with { History = [.. context.History, $"Final Answer: {finalAnswer}"] };

                _logger.LogInformation("ReAct completed with final answer after {Iterations} iteration(s)", iteration + 1);
                return new AgentResponse(finalAnswer, true, context, steps);
            }

            // Parse action
            var (toolName, actionInput) = ExtractAction(llmOutput);
            if (toolName is null)
            {
                stopwatch.Stop();
                _logger.LogWarning("ReAct iteration {Iteration}: could not parse action from LLM output", iteration);
                steps.Add(new AgentStep(Name, llmOutput, "Failed to parse action", stopwatch.Elapsed, false));
                messages.Add(new ChatMessage
                {
                    Role = "user",
                    Content = "Observation: I could not understand your response. Please follow the exact format: Thought/Action/Action Input or Thought/Final Answer."
                });
                continue;
            }

            // Execute tool
            var tool = _tools.FirstOrDefault(t => t.Name.Equals(toolName, StringComparison.OrdinalIgnoreCase));
            if (tool is null)
            {
                stopwatch.Stop();
                var errorMsg = $"Tool '{toolName}' not found. Available tools: {string.Join(", ", _tools.Select(t => t.Name))}";
                _logger.LogWarning("ReAct iteration {Iteration}: {Error}", iteration, errorMsg);
                steps.Add(new AgentStep(Name, toolName, errorMsg, stopwatch.Elapsed, false));
                messages.Add(new ChatMessage { Role = "user", Content = $"Observation: {errorMsg}" });
                continue;
            }

            var parameters = ParseActionInput(actionInput);
            ToolResult toolResult;

            try
            {
                toolResult = await tool.ExecuteAsync(parameters, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tool '{ToolName}' threw an exception during ReAct execution", toolName);
                toolResult = new ToolResult(false, Error: ex.Message);
            }

            stopwatch.Stop();

            var observation = toolResult.Success
                ? toolResult.Data?.ToString() ?? "Tool executed successfully with no output."
                : $"Error: {toolResult.Error}";

            steps.Add(new AgentStep(toolName, actionInput ?? string.Empty, observation, stopwatch.Elapsed, toolResult.Success));
            context = context with { History = [.. context.History, $"Action: {toolName} -> {observation}"] };

            messages.Add(new ChatMessage { Role = "user", Content = $"Observation: {observation}" });

            _logger.LogDebug("ReAct iteration {Iteration}: tool '{ToolName}' result success={Success}", iteration, toolName, toolResult.Success);
        }

        _logger.LogWarning("ReAct loop exhausted maximum iterations ({Max})", _maxIterations);
        return new AgentResponse(
            "Maximum iterations reached without a final answer.",
            false,
            context,
            steps);
    }

    private string BuildToolDescriptions()
    {
        var sb = new StringBuilder();
        foreach (var tool in _tools)
        {
            sb.AppendLine($"- {tool.Name}: {tool.Description}");
            foreach (var param in tool.Schema.Parameters)
            {
                var requiredTag = param.Required ? " (required)" : " (optional)";
                sb.AppendLine($"  - {param.Name} ({param.Type}): {param.Description}{requiredTag}");
            }
        }
        return sb.ToString();
    }

    private static string? ExtractFinalAnswer(string output)
    {
        var match = FinalAnswerRegex().Match(output);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static (string? ToolName, string? ActionInput) ExtractAction(string output)
    {
        var actionMatch = ActionRegex().Match(output);
        if (!actionMatch.Success)
        {
            return (null, null);
        }

        var toolName = actionMatch.Groups[1].Value.Trim();
        var inputMatch = ActionInputRegex().Match(output);
        var actionInput = inputMatch.Success ? inputMatch.Groups[1].Value.Trim() : null;

        return (toolName, actionInput);
    }

    private static Dictionary<string, object> ParseActionInput(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new Dictionary<string, object>();
        }

        try
        {
            var parsed = JsonSerializer.Deserialize(input, ReActJsonContext.Default.DictionaryStringObject);
            return parsed ?? new Dictionary<string, object>();
        }
        catch
        {
            return new Dictionary<string, object> { ["input"] = input };
        }
    }

    [GeneratedRegex(@"Final Answer:\s*(.+)", RegexOptions.Singleline)]
    private static partial Regex FinalAnswerRegex();

    [GeneratedRegex(@"Action:\s*(.+)")]
    private static partial Regex ActionRegex();

    [GeneratedRegex(@"Action Input:\s*(.+)", RegexOptions.Singleline)]
    private static partial Regex ActionInputRegex();
}

/// <summary>
/// AOT-compatible JSON serializer context for ReAct strategy.
/// </summary>
[JsonSerializable(typeof(Dictionary<string, object>))]
internal partial class ReActJsonContext : JsonSerializerContext { }
