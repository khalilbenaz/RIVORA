namespace RVR.Framework.Application.CQRS;

using MediatR;

/// <summary>
/// Marker interface for commands.
/// </summary>
public interface ICommand : IRequest<Unit>
{
}

/// <summary>
/// Marker interface for commands that return a result.
/// </summary>
/// <typeparam name="TResponse">The type of response the command returns.</typeparam>
public interface ICommand<TResponse> : IRequest<TResponse>
{
}