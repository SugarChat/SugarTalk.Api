using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class CutMeetingRecordUrlCommandHandler : ICommandHandler<CutMeetingRecordUrlCommand, CutMeetingRecordUrlResponse>
{
    private readonly IMeetingService _meetingService;

    public CutMeetingRecordUrlCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<CutMeetingRecordUrlResponse> Handle(IReceiveContext<CutMeetingRecordUrlCommand> context, CancellationToken cancellationToken)
    {
        return await _meetingService.CutMeetingRecordUrlAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}