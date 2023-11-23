using System.Threading;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using System.Threading.Tasks;
using SugarTalk.Messages.Events.Meeting;

namespace SugarTalk.Core.Handlers.EventHandlers.Meeting;

public class UpdateMeetingEventHandler : IEventHandler<MeetingUpdatedEvent>
{
    public Task Handle(IReceiveContext<MeetingUpdatedEvent> context, CancellationToken cancellationToken)
    {
       return Task.CompletedTask;
    }
}