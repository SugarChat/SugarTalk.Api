using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class MeetingRecordRestartCommandHandler : ICommandHandler<MeetingRecordRestartCommand>
{
    private readonly IMeetingService _meetingService;

    public MeetingRecordRestartCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task Handle(IReceiveContext<MeetingRecordRestartCommand> context, CancellationToken cancellationToken)
    {
        await _meetingService.GeneralMeetingRecordRestartAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}