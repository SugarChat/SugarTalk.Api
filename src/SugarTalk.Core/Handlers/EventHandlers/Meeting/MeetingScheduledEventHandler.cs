using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Events.Meeting;

namespace SugarTalk.Core.Handlers.EventHandlers.Meeting;

public class MeetingScheduledEventHandler : IEventHandler<MeetingScheduledEvent>
{
    public Task Handle(IReceiveContext<MeetingScheduledEvent> context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}