using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Events.Meeting;
using SugarTalk.Core.Services.EventHandling;

namespace SugarTalk.Core.Handlers.EventHandlers.Meeting;

public class AudioChangedEventHandler : IEventHandler<AudioChangedEvent>
{
    private readonly IEventHandlingService _eventHandlingService;

    public AudioChangedEventHandler(IEventHandlingService eventHandlingService)
    {
        _eventHandlingService = eventHandlingService;
    }

    public async Task Handle(IReceiveContext<AudioChangedEvent> context, CancellationToken cancellationToken)
    {
        await _eventHandlingService.HandlingEventAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
