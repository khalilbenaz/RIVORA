using MassTransit;
using RVR.Microservices.Shared.Application.Events;

namespace RVR.Microservices.Shared.Infrastructure.Messaging;

public interface IEventBus
{
    Task PublishAsync<T>(T eventMessage, CancellationToken ct = default) where T : IntegrationEvent;
}

public class MassTransitEventBus : IEventBus
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitEventBus(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishAsync<T>(T eventMessage, CancellationToken ct = default) where T : IntegrationEvent
    {
        await _publishEndpoint.Publish(eventMessage, ct);
    }
}
