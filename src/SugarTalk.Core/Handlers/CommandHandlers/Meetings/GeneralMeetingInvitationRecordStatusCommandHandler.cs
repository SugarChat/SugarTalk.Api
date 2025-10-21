using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class GeneralMeetingInvitationRecordStatusCommandHandler : ICommandHandler<GeneralMeetingInvitationRecordStatusCommand>
{
    private readonly IMeetingService _meetingService;

    public GeneralMeetingInvitationRecordStatusCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task Handle(IReceiveContext<GeneralMeetingInvitationRecordStatusCommand> context, CancellationToken cancellationToken)
    {
        await _meetingService.GeneralMeetingInvitationRecordStatusAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}