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
    private readonly IIdentityService _identityService;

    public GuestValidatorMiddlewareSpecification(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public bool ShouldExecute(TContext context, CancellationToken cancellationToken)
    {
        return context.Message is ICommand or IRequest;
    }

    public async Task BeforeExecute(TContext context, CancellationToken cancellationToken)
    {
        if (!ShouldExecute(context, cancellationToken)) return;
        
        var currentUser = await _identityService.GetCurrentUserAsync(cancellationToken: cancellationToken);
        
        if (currentUser?.Issuer == UserAccountIssuer.Guest && !context.Message.GetType().GetAttribute<AllowGuestAccessAttribute>().Any()) 
            throw new GuestIsNotAllowException();
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