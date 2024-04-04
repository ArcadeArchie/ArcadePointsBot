using ArcadePointsBot.Common.Primitives;

namespace ArcadePointsBot.Common.Abstractions.Messaging;

internal interface ICommand<out TResponse> : Mediator.ICommand<TResponse>
    where TResponse : Result;

internal interface IQuery<TResponse> : Mediator.IQuery<TResponse>
    where TResponse : Result;

/// <summary>
/// Represents the query interface.
/// </summary>
/// <typeparam name="TQuery">The query type.</typeparam>
/// <typeparam name="TResponse">The query response type.</typeparam>
internal interface IQueryHandler<in TQuery, TResponse> : Mediator.IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
    where TResponse : Result;

/// <summary>
/// Represents the command interface.
/// </summary>
/// <typeparam name="TQuery">The query type.</typeparam>
/// <typeparam name="TResponse">The query response type.</typeparam>
internal interface ICommandHandler<in TCommand, TResponse>
    : Mediator.ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
    where TResponse : Result;
