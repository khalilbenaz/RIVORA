using MediatR;
using Microsoft.Extensions.Logging;

namespace RVR.Framework.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that catches and logs any unhandled exceptions
/// thrown by downstream handlers, then re-throws so callers still receive the error.
/// </summary>
public sealed class UnhandledExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> _logger;

    public UnhandledExceptionBehavior(ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception for request {RequestName} {@Request}",
                typeof(TRequest).Name,
                request);
            throw;
        }
    }
}
