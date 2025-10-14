namespace RVR.Framework.Application.CQRS;

using MediatR;

/// <summary>
/// Handler interface for commands that do not return a result.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle.</typeparam>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Unit> where TCommand : ICommand
{
}

/// <summary>
/// Handler interface for commands that return a result.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle.</typeparam>
/// <typeparam name="TResponse">The type of response the command returns.</typeparam>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>
{
}
