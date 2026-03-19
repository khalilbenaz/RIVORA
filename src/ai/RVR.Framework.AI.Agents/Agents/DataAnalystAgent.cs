using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RVR.Framework.AI.Abstractions;
using RVR.Framework.AI.Agents.Configuration;

namespace RVR.Framework.AI.Agents.Agents;

/// <summary>
/// A pre-built agent that analyzes data or text and extracts structured insights
/// using a configured language model.
/// </summary>
public sealed class DataAnalystAgent : IAgent
{
    private readonly IChatClient _chatClient;
    private readonly AgentOptions _options;
    private readonly ILogger<DataAnalystAgent> _logger;

    private const string DefaultSystemPrompt =
        """
        You are an expert data analyst. Analyze the provided data or text and extract meaningful insights.
        Structure your response as follows:

        1. **Overview**: A brief summary of the data/text provided.
        2. **Key Findings**: The most important patterns, trends, or facts identified.
        3. **Statistical Observations**: Any notable numerical patterns, distributions, or anomalies.
        4. **Correlations & Relationships**: Connections between different data points or concepts.
        5. **Recommendations**: Actionable suggestions based on the analysis.
        6. **Caveats**: Any limitations, data quality issues, or assumptions made.

        Be precise, data-driven, and avoid speculation beyond what the data supports.
        If the input contains structured data (CSV, JSON, tables), parse it accordingly.
        """;

    /// <summary>
    /// Initializes a new instance of <see cref="DataAnalystAgent"/>.
    /// </summary>
    /// <param name="chatClient">The LLM client used for data analysis.</param>
    /// <param name="options">The agent configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    public DataAnalystAgent(
        IChatClient chatClient,
        IOptions<AgentOptions> options,
        ILogger<DataAnalystAgent> logger)
    {
        ArgumentNullException.ThrowIfNull(chatClient);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _chatClient = chatClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public string Name => "DataAnalyst";

    /// <inheritdoc />
    public string Description => "Analyzes data or text and extracts structured insights, patterns, and recommendations using a configured language model.";

    /// <inheritdoc />
    public async Task<AgentResponse> ExecuteAsync(AgentRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var context = request.Context ?? new AgentContext(new Dictionary<string, object>(), []);
        var stopwatch = Stopwatch.StartNew();

        var dataType = request.Parameters?.TryGetValue("dataType", out var dt) == true
            ? dt.ToString()
            : "text";

        _logger.LogInformation("DataAnalystAgent processing data (length={Length}, type={DataType})", request.Input.Length, dataType);

        try
        {
            var chatRequest = new ChatRequest
            {
                Model = _options.DefaultModel,
                SystemPrompt = DefaultSystemPrompt,
                Messages =
                [
                    new ChatMessage
                    {
                        Role = "user",
                        Content = $"Please analyze the following {dataType} data and extract insights:\n\n{request.Input}"
                    }
                ],
                Temperature = 0.3f,
                MaxTokens = 2048
            };

            var response = await _chatClient.ChatAsync(chatRequest, ct).ConfigureAwait(false);
            stopwatch.Stop();

            var step = new AgentStep(Name, request.Input, response.Content, stopwatch.Elapsed, true);
            context = context with { History = [.. context.History, $"Analyzed {dataType} data ({request.Input.Length} chars)"] };

            _logger.LogInformation("DataAnalystAgent completed in {Duration}ms", stopwatch.ElapsedMilliseconds);
            return new AgentResponse(response.Content, true, context, [step]);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "DataAnalystAgent failed");

            var step = new AgentStep(Name, request.Input, ex.Message, stopwatch.Elapsed, false);
            return new AgentResponse(ex.Message, false, context, [step]);
        }
    }
}
