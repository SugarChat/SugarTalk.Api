using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class UpdateMeetingCommandHandler : ICommandHandler<UpdateMeetingCommand, UpdateMeetingResponse>
{
    private readonly IMeetingService _meetingService;

    public UpdateMeetingCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<UpdateMeetingResponse> Handle(IReceiveContext<UpdateMeetingCommand> context, CancellationToken cancellationToken)
    {
        return await _meetingService.UpdateMeetingAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}