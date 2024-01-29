using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Speech;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings.User;

public class AddOrUpdateMeetingUserSettingCommandHandler : ICommandHandler<AddOrUpdateMeetingUserSettingCommand, AddOrUpdateMeetingUserSettingResponse>
{
    private readonly IMeetingService _meetingService;

    public AddOrUpdateMeetingUserSettingCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<AddOrUpdateMeetingUserSettingResponse> Handle(IReceiveContext<AddOrUpdateMeetingUserSettingCommand> context, CancellationToken cancellationToken)
    {
        var @event = await _meetingService.AddOrUpdateMeetingUserSettingAsync(context.Message, cancellationToken).ConfigureAwait(false);
        
        await context.PublishAsync(@event, cancellationToken).ConfigureAwait(false);

        return new AddOrUpdateMeetingUserSettingResponse();
    }
}