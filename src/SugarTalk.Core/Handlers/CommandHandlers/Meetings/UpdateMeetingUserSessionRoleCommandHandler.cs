using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class UpdateMeetingUserSessionRoleCommandHandler : ICommandHandler<UpdateMeetingUserSessionRoleCommand, UpdateMeetingUserSessionRoleResponse>
{
    private readonly IMeetingService _meetingService;
    
    public UpdateMeetingUserSessionRoleCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<UpdateMeetingUserSessionRoleResponse> Handle(IReceiveContext<UpdateMeetingUserSessionRoleCommand> context, CancellationToken cancellationToken)
    {
        return await _meetingService.UpdateMeetingUserSessionRoleAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}