using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Events.Meeting.Speech;

namespace SugarTalk.Core.Handlers.EventHandlers.Meeting.Speech;

public class MeetingSpeechUpdatedEventHandler : IEventHandler<MeetingSpeechUpdatedEvent>
{
    public Task Handle(IReceiveContext<MeetingSpeechUpdatedEvent> context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}