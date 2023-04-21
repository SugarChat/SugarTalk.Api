using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Events.Account;

namespace SugarTalk.Core.Handlers.EventHandlers.Account;

public class UserAccountRegisteredEventHandler : IEventHandler<UserAccountRegisteredEvent>
{
    public Task Handle(IReceiveContext<UserAccountRegisteredEvent> context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}