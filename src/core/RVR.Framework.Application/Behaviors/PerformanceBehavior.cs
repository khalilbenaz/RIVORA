using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace RVR.Framework.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs a warning when a request handler
/// takes longer than the configured threshold (default 500 ms).
/// </summary>
public sealed class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const long WarningThresholdMs = 500;

    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > WarningThresholdMs)
        {
            _logger.LogWarning(
                "Long-running request detected: {RequestName} took {ElapsedMs}ms (threshold: {ThresholdMs}ms) {@Request}",
                typeof(TRequest).Name,
                stopwatch.ElapsedMilliseconds,
                WarningThresholdMs,
                request);
        }

        return response;
    }
}
