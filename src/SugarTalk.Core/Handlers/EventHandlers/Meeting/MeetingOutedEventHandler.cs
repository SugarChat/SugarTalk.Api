using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Events.Meeting;

namespace SugarTalk.Core.Handlers.EventHandlers.Meeting;

public class MeetingOutedEventHandler : IEventHandler<MeetingOutedEvent>
{
    public Task Handle(IReceiveContext<MeetingOutedEvent> context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}