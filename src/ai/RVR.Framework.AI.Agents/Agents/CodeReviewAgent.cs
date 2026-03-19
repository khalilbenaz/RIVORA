using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RVR.Framework.AI.Abstractions;
using RVR.Framework.AI.Agents.Configuration;

namespace RVR.Framework.AI.Agents.Agents;

/// <summary>
/// A pre-built agent that reviews source code using a configured LLM, providing
/// feedback on code quality, potential bugs, security issues, and best practices.
/// </summary>
public sealed class CodeReviewAgent : IAgent
{
    private readonly IChatClient _chatClient;
    private readonly AgentOptions _options;
    private readonly ILogger<CodeReviewAgent> _logger;

    private const string DefaultSystemPrompt =
        """
        You are a senior software engineer performing a code review. Analyze the provided code and provide structured feedback covering:

        1. **Bugs & Errors**: Identify any logical errors, null reference risks, race conditions, or incorrect behavior.
        2. **Security**: Flag potential security vulnerabilities (SQL injection, XSS, hardcoded secrets, etc.).
        3. **Performance**: Highlight performance concerns such as unnecessary allocations, N+1 queries, or blocking calls.
        4. **Best Practices**: Suggest improvements for readability, maintainability, and adherence to SOLID principles.
        5. **Overall Assessment**: Provide a brief summary with a quality rating (Excellent / Good / Needs Improvement / Critical Issues).

        Be specific and actionable. Reference line numbers or code snippets where applicable.
        If the code is well-written, acknowledge that.
        """;

    /// <summary>
    /// Initializes a new instance of <see cref="CodeReviewAgent"/>.
    /// </summary>
    /// <param name="chatClient">The LLM client used for code review.</param>
    /// <param name="options">The agent configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    public CodeReviewAgent(
        IChatClient chatClient,
        IOptions<AgentOptions> options,
        ILogger<CodeReviewAgent> logger)
    {
        ArgumentNullException.ThrowIfNull(chatClient);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _chatClient = chatClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public string Name => "CodeReview";

    /// <inheritdoc />
    public string Description => "Reviews source code for bugs, security issues, performance concerns, and best practices using a configured language model.";

    /// <inheritdoc />
    public async Task<AgentResponse> ExecuteAsync(AgentRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var context = request.Context ?? new AgentContext(new Dictionary<string, object>(), []);
        var stopwatch = Stopwatch.StartNew();

        var language = request.Parameters?.TryGetValue("language", out var lang) == true
            ? lang.ToString()
            : "auto-detect";

        _logger.LogInformation("CodeReviewAgent processing code (length={Length}, language={Language})", request.Input.Length, language);

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
                        Content = $"Please review the following {language} code:\n\n```\n{request.Input}\n```"
                    }
                ],
                Temperature = 0.2f,
                MaxTokens = 2048
            };

            var response = await _chatClient.ChatAsync(chatRequest, ct).ConfigureAwait(false);
            stopwatch.Stop();

            var step = new AgentStep(Name, request.Input, response.Content, stopwatch.Elapsed, true);
            context = context with { History = [.. context.History, $"Reviewed code ({language}, {request.Input.Length} chars)"] };

            _logger.LogInformation("CodeReviewAgent completed in {Duration}ms", stopwatch.ElapsedMilliseconds);
            return new AgentResponse(response.Content, true, context, [step]);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "CodeReviewAgent failed");

            var step = new AgentStep(Name, request.Input, ex.Message, stopwatch.Elapsed, false);
            return new AgentResponse(ex.Message, false, context, [step]);
        }
    }
}
