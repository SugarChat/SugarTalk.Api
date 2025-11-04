using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class CreateMeetingInvitationRecordsCommandHandler : ICommandHandler<CreateMeetingInvitationRecordsCommand, CreateMeetingInvitationRecordsResponse>
{
    private readonly IMeetingService _meetingService;
    
    public CreateMeetingInvitationRecordsCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task<CreateMeetingInvitationRecordsResponse> Handle(IReceiveContext<CreateMeetingInvitationRecordsCommand> context, CancellationToken cancellationToken)
    {
        return await _meetingService.CreateMeetingInvitationRecordsAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}