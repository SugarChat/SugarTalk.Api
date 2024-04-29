using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Attributes;
using SugarTalk.Messages.Commands.Meetings.Speak;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings.User;

public class AddOrUpdateChatRoomSettingCommandHandler : ICommandHandler<AddOrUpdateChatRoomSettingCommand, AddOrUpdateChatRoomSettingResponse>
{
    private readonly IMeetingService _meetingService;

    public AddOrUpdateChatRoomSettingCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task<AddOrUpdateChatRoomSettingResponse> Handle(IReceiveContext<AddOrUpdateChatRoomSettingCommand> context, CancellationToken cancellationToken)
    {
        var @event = await _meetingService.AddOrUpdateChatRoomSettingAsync(context.Message, cancellationToken).ConfigureAwait(false);
        
        await context.PublishAsync(@event, cancellationToken).ConfigureAwait(false);

        return new AddOrUpdateChatRoomSettingResponse();
    }
}