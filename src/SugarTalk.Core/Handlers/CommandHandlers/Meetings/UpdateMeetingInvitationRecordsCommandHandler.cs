using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class UpdateMeetingInvitationRecordsCommandHandler : ICommandHandler<UpdateMeetingInvitationRecordsCommand, UpdateMeetingInvitationRecordsResponse>
{
    private readonly IMeetingService _meetingService;

    public UpdateMeetingInvitationRecordsCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<UpdateMeetingInvitationRecordsResponse> Handle(IReceiveContext<UpdateMeetingInvitationRecordsCommand> context, CancellationToken cancellationToken)
    {
        return await _meetingService.UpdateMeetingInvitationRecordAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}