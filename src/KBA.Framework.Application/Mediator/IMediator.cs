namespace KBA.Framework.Application.Mediator;

/// <summary>
/// Defines a mediator for publishing commands and queries.
/// Provides a simplified abstraction over the full MediatR library.
/// </summary>
public interface IMediator
{
    Task<TResponse> Send<TResponse>(KBA.Framework.Application.CQRS.ICommand<TResponse> command, CancellationToken cancellationToken = default);
    Task Send(KBA.Framework.Application.CQRS.ICommand command, CancellationToken cancellationToken = default);
    Task<TResponse> Send<TResponse>(KBA.Framework.Application.CQRS.IQuery<TResponse> query, CancellationToken cancellationToken = default);
    Task Publish(object notification, CancellationToken cancellationToken = default);
}