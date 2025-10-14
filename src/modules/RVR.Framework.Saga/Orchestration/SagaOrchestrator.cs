namespace RVR.Framework.Saga.Orchestration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RVR.Framework.Saga.Abstractions;
using RVR.Framework.Saga.Models;

/// <summary>
/// Dispatches saga events to the correct saga handler and manages saga lifecycle.
/// </summary>
public class SagaOrchestrator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISagaStore _sagaStore;
    private readonly ILogger<SagaOrchestrator> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="SagaOrchestrator"/>.
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving sagas.</param>
    /// <param name="sagaStore">The saga persistence store.</param>
    /// <param name="logger">Logger instance.</param>
    public SagaOrchestrator(
        IServiceProvider serviceProvider,
        ISagaStore sagaStore,
        ILogger<SagaOrchestrator> logger)
    {
        _serviceProvider = serviceProvider;
        _sagaStore = sagaStore;
        _logger = logger;
    }

    /// <summary>
    /// Dispatches an event to all registered sagas that can handle it.
    /// Creates a new saga instance if one does not exist for the correlation ID.
    /// </summary>
    /// <typeparam name="TData">The saga data type.</typeparam>
    /// <typeparam name="TEvent">The event type.</typeparam>
    /// <param name="event">The event to dispatch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task DispatchAsync<TData, TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TData : class, new()
        where TEvent : ISagaEvent
    {
        var sagas = _serviceProvider.GetServices<ISaga<TData>>();

        foreach (var saga in sagas)
        {
            if (!saga.CanHandle(typeof(TEvent)))
            {
                continue;
            }

            _logger.LogDebug("Dispatching event '{EventType}' to saga '{SagaName}' with correlation '{CorrelationId}'",
                typeof(TEvent).Name, saga.SagaName, @event.CorrelationId);

            var state = await _sagaStore.LoadAsync<TData>(@event.CorrelationId, cancellationToken);

            if (state == null)
            {
                state = new SagaState<TData>
                {
                    Id = @event.CorrelationId,
                    Status = SagaStatus.InProgress,
                    StartedAt = DateTime.UtcNow
                };
            }

            try
            {
                state = await saga.HandleAsync(state, @event, cancellationToken);
                await _sagaStore.SaveAsync(state, cancellationToken);

                _logger.LogInformation("Saga '{SagaName}' processed event '{EventType}'. Current step: '{Step}', Status: '{Status}'",
                    saga.SagaName, typeof(TEvent).Name, state.CurrentStep, state.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Saga '{SagaName}' failed while handling event '{EventType}'",
                    saga.SagaName, typeof(TEvent).Name);

                await _sagaStore.MarkFailedAsync<TData>(@event.CorrelationId, ex.Message, cancellationToken);
                throw;
            }
        }
    }
}
