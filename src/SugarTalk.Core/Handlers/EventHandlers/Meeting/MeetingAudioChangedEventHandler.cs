using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Events.Meeting;

namespace SugarTalk.Core.Handlers.EventHandlers.Meeting;

public class MeetingAudioChangedEventHandler : IEventHandler<AudioChangedEvent>
{
    public Task Handle(IReceiveContext<AudioChangedEvent> context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}