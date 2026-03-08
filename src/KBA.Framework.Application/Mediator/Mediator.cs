namespace KBA.Framework.Application.Mediator;

using MediatR;

/// <summary>
/// In-memory mediator implementation that wraps MediatR.
/// Provides a simplified interface for sending commands and queries.
/// </summary>
public class Mediator : IMediator
{
    private readonly IMediator _mediator;

    public Mediator(IMediator mediator) => _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    public Task<TResponse> Send<TResponse>(KBA.Framework.Application.CQRS.ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        if (command == null) throw new ArgumentNullException(nameof(command));
        return _mediator.Send(command, cancellationToken);
    }

    public async Task Send(KBA.Framework.Application.CQRS.ICommand command, CancellationToken cancellationToken = default)
    {
        if (command == null) throw new ArgumentNullException(nameof(command));
        await _mediator.Send(command, cancellationToken);
    }

    public Task<TResponse> Send<TResponse>(KBA.Framework.Application.CQRS.IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        if (query == null) throw new ArgumentNullException(nameof(query));
        return _mediator.Send(query, cancellationToken);
    }

    public Task Publish(object notification, CancellationToken cancellationToken = default)
    {
        if (notification == null) throw new ArgumentNullException(nameof(notification));
        return _mediator.Publish(notification, cancellationToken);
    }

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        if (notification == null) throw new ArgumentNullException(nameof(notification));
        return _mediator.Publish(notification, cancellationToken);
    }
}
