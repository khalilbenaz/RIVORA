namespace KBA.Framework.Application.CQRS;

using MediatR;

/// <summary>
/// Marker interface for queries.
/// A query represents a request for data and should not cause any side effects.
/// </summary>
/// <typeparam name="TResponse">The type of response the query returns.</typeparam>
public interface IQuery<ot TResponse> : IRequest<TResponse>
{
}