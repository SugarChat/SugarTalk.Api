using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Events.Meeting;
using SugarTalk.Messages.Events.Meeting.Speech;

namespace SugarTalk.Core.Handlers.EventHandlers.Meeting.Speech;

public class MeetingAudioSavedEventHandler : IEventHandler<MeetingAudioSavedEvent>
{
    public Task Handle(IReceiveContext<MeetingAudioSavedEvent> context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}