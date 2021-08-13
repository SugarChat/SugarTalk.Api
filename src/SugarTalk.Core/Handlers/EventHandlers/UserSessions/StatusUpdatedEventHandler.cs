using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Events.UserSessions;

namespace SugarTalk.Core.Handlers.EventHandlers.UserSessions
{
    public class StatusUpdatedEventHandler : IEventHandler<StatusUpdatedEvent>
    {
        public Task Handle(IReceiveContext<StatusUpdatedEvent> context, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}