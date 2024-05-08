using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings.Summary;

public class ReStartMeetingRecordingCommandHandler : ICommandHandler<ReStartMeetingRecordingCommand>
{
    private readonly IMeetingService _meetingService;

    public ReStartMeetingRecordingCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task Handle(IReceiveContext<ReStartMeetingRecordingCommand> context, CancellationToken cancellationToken)
    {
        await _meetingService.ReStartMeetingRecordingAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}