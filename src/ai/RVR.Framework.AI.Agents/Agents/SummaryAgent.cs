using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RVR.Framework.AI.Abstractions;
using RVR.Framework.AI.Agents.Configuration;

namespace RVR.Framework.AI.Agents.Agents;

/// <summary>
/// A pre-built agent that summarizes text using a configured LLM.
/// </summary>
public sealed class SummaryAgent : IAgent
{
    private readonly IChatClient _chatClient;
    private readonly AgentOptions _options;
    private readonly ILogger<SummaryAgent> _logger;

    private const string DefaultSystemPrompt =
        """
        You are a professional summarization assistant. Your task is to create clear, concise,
        and accurate summaries of the provided text. Preserve key facts, figures, and conclusions.
        Adapt the summary length proportionally to the input length.
        If the text is very short, provide a brief one-sentence summary.
        """;

    /// <summary>
    /// Initializes a new instance of <see cref="SummaryAgent"/>.
    /// </summary>
    /// <param name="chatClient">The LLM client used for summarization.</param>
    /// <param name="options">The agent configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    public SummaryAgent(
        IChatClient chatClient,
        IOptions<AgentOptions> options,
        ILogger<SummaryAgent> logger)
    {
        ArgumentNullException.ThrowIfNull(chatClient);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _chatClient = chatClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public string Name => "Summary";

    /// <inheritdoc />
    public string Description => "Summarizes text content using a configured language model.";

    /// <inheritdoc />
    public async Task<AgentResponse> ExecuteAsync(AgentRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var context = request.Context ?? new AgentContext(new Dictionary<string, object>(), []);
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("SummaryAgent processing input (length={Length})", request.Input.Length);

        try
        {
            var chatRequest = new ChatRequest
            {
                Model = _options.DefaultModel,
                SystemPrompt = DefaultSystemPrompt,
                Messages = [new ChatMessage { Role = "user", Content = $"Please summarize the following text:\n\n{request.Input}" }],
                Temperature = 0.3f,
                MaxTokens = 1024
            };

            var response = await _chatClient.ChatAsync(chatRequest, ct).ConfigureAwait(false);
            stopwatch.Stop();

            var step = new AgentStep(Name, request.Input, response.Content, stopwatch.Elapsed, true);
            context = context with { History = [.. context.History, $"Summarized {request.Input.Length} chars"] };

            _logger.LogInformation("SummaryAgent completed in {Duration}ms", stopwatch.ElapsedMilliseconds);
            return new AgentResponse(response.Content, true, context, [step]);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "SummaryAgent failed");

            var step = new AgentStep(Name, request.Input, ex.Message, stopwatch.Elapsed, false);
            return new AgentResponse(ex.Message, false, context, [step]);
        }
    }
}
