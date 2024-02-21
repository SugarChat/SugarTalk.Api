using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;

namespace SugarTalk.Core.Handlers.EventHandlers.Meeting;

public class MeetingRecordingStartedEventHandler : IEventHandler<MeetingRecordingStartedEvent>
{
    public Task Handle(IReceiveContext<MeetingRecordingStartedEvent> context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}