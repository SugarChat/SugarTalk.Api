using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Events.Meeting.User;

namespace SugarTalk.Core.Handlers.EventHandlers.Meeting.User;

public class ChatRoomSettingAddOrUpdateEventHandler : IEventHandler<ChatRoomSettingAddOrUpdateEvent>
{
    public Task Handle(IReceiveContext<ChatRoomSettingAddOrUpdateEvent> context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}