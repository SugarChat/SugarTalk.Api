using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings.Summary;

public class ReStartMeetingRecordingCommandHandler : ICommandHandler<ReStartMeetingRecordingCommand, ReStartMeetingRecordingResponse>
{
    private readonly IMeetingService _meetingService;

    public ReStartMeetingRecordingCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<ReStartMeetingRecordingResponse> Handle(IReceiveContext<ReStartMeetingRecordingCommand> context, CancellationToken cancellationToken)
    {
        return await _meetingService.ReStartMeetingRecordingAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}