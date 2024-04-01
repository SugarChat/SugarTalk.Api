using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class UpdateInProcessMeetingCommandHandler : ICommandHandler<UpdateInProcessMeetingCommand, UpdateInProcessMeetingResponse>
{
    private readonly IMeetingService _meetingService;

    public UpdateInProcessMeetingCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<UpdateInProcessMeetingResponse> Handle(IReceiveContext<UpdateInProcessMeetingCommand> context, CancellationToken cancellationToken)
    {
        return await _meetingService.UpdateMeetingChatResponseAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}