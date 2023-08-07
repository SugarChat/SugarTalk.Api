using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Events.Meeting;
using SugarTalk.Core.Services.EventHandling;

namespace SugarTalk.Core.Handlers.EventHandlers.Meeting;

public class ScreenSharedEventHandler : IEventHandler<ScreenSharedEvent>
{
    private readonly IEventHandlingService _eventHandlingService;

    public ScreenSharedEventHandler(IEventHandlingService eventHandlingService)
    {
        _eventHandlingService = eventHandlingService;
    }

    public async Task Handle(IReceiveContext<ScreenSharedEvent> context, CancellationToken cancellationToken)
    {
        await _eventHandlingService.HandlingEventAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}