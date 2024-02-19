using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using MassTransit.Internals;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;
using SugarTalk.Core.Services.Identity;
using SugarTalk.Messages.Attributes;
using SugarTalk.Messages.Enums.Account;

namespace SugarTalk.Core.Middlewares.GuestValidator;

public class GuestValidatorMiddlewareSpecification<TContext> : IPipeSpecification<TContext>
    where TContext : IContext<IMessage>
{
    private readonly ICurrentUser _currentUser;

    public GuestValidatorMiddlewareSpecification(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public bool ShouldExecute(TContext context, CancellationToken cancellationToken)
    {
        return _currentUser.AuthType == UserAccountIssuer.Guest && context.Message.GetType().GetAttribute<GuestValidatorAttribute>().Any();
    }

    public async Task BeforeExecute(TContext context, CancellationToken cancellationToken)
    {
        if (!ShouldExecute(context, cancellationToken)) throw new GuestIsNotAllowException();
    }

    public Task Execute(TContext context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task AfterExecute(TContext context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task OnException(Exception ex, TContext context)
    {
        ExceptionDispatchInfo.Capture(ex).Throw();
        throw ex;
    }
}