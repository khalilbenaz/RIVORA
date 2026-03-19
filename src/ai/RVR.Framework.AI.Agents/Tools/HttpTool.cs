using System.Text;
using Microsoft.Extensions.Logging;

namespace RVR.Framework.AI.Agents.Tools;

/// <summary>
/// A tool that makes HTTP requests, allowing agents to interact with web APIs.
/// </summary>
public sealed class HttpTool : ITool
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpTool> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="HttpTool"/>.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory for creating clients.</param>
    /// <param name="logger">The logger instance.</param>
    public HttpTool(IHttpClientFactory httpClientFactory, ILogger<HttpTool> logger)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(logger);

        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public string Name => "http";

    /// <inheritdoc />
    public string Description => "Makes HTTP requests to a specified URL and returns the response body.";

    /// <inheritdoc />
    public ToolSchema Schema => new(
        Name,
        Description,
        [
            new ToolParameter("url", "string", "The URL to send the request to."),
            new ToolParameter("method", "string", "The HTTP method (GET, POST, PUT, DELETE). Defaults to GET.", Required: false),
            new ToolParameter("body", "string", "The request body for POST/PUT requests.", Required: false),
            new ToolParameter("contentType", "string", "The Content-Type header value. Defaults to application/json.", Required: false),
        ]);

    /// <inheritdoc />
    public async Task<ToolResult> ExecuteAsync(Dictionary<string, object> parameters, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        if (!parameters.TryGetValue("url", out var urlObj) || urlObj is not string url || string.IsNullOrWhiteSpace(url))
        {
            return new ToolResult(false, Error: "The 'url' parameter is required.");
        }

        var method = parameters.TryGetValue("method", out var methodObj)
            ? methodObj.ToString()?.ToUpperInvariant() ?? "GET"
            : "GET";

        var body = parameters.TryGetValue("body", out var bodyObj) ? bodyObj.ToString() : null;
        var contentType = parameters.TryGetValue("contentType", out var ctObj) ? ctObj.ToString() : "application/json";

        _logger.LogInformation("HttpTool executing {Method} {Url}", method, url);

        try
        {
            using var client = _httpClientFactory.CreateClient("AgentHttpTool");
            using var request = new HttpRequestMessage(new HttpMethod(method), url);

            if (body is not null && method is "POST" or "PUT" or "PATCH")
            {
                request.Content = new StringContent(body, Encoding.UTF8, contentType!);
            }

            using var response = await client.SendAsync(request, ct).ConfigureAwait(false);
            var responseBody = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            _logger.LogDebug("HttpTool received {StatusCode} ({Length} chars)", (int)response.StatusCode, responseBody.Length);

            if (!response.IsSuccessStatusCode)
            {
                return new ToolResult(false, Data: responseBody, Error: $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}");
            }

            return new ToolResult(true, Data: responseBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HttpTool failed for {Method} {Url}", method, url);
            return new ToolResult(false, Error: ex.Message);
        }
    }
}
