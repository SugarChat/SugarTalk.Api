using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class UpdateMeetingUserSessionTypeCommandHandler : ICommandHandler<UpdateMeetingUserSessionTypeCommand, UpdateMeetingUserSessionTypeResponse>
{
    private readonly IMeetingService _meetingService;
    
    public UpdateMeetingUserSessionTypeCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<UpdateMeetingUserSessionTypeResponse> Handle(IReceiveContext<UpdateMeetingUserSessionTypeCommand> context, CancellationToken cancellationToken)
    {
        return await _meetingService.UpdateMeetingUserSessionTypAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}