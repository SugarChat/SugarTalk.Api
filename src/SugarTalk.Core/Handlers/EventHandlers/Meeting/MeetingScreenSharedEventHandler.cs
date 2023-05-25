using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Events.Meeting;

namespace SugarTalk.Core.Handlers.EventHandlers.Meeting;

public class MeetingScreenSharedEventHandler : IEventHandler<ScreenSharedEvent>
{
    public Task Handle(IReceiveContext<ScreenSharedEvent> context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}