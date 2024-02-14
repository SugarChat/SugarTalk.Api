using System.Threading;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using System.Threading.Tasks;
using SugarTalk.Messages.Events.Meeting.Speak;

namespace SugarTalk.Core.Handlers.EventHandlers.Meeting.Speak;

public class MeetingSpeakRecordedEventHandler : IEventHandler<MeetingSpeakRecordedEvent>
{
    public Task Handle(IReceiveContext<MeetingSpeakRecordedEvent> context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}