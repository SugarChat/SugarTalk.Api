using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class MeetingSummaryPdfExportCommandHandler : ICommandHandler<MeetingRecordPdfExportCommand, MeetingRecordPdfExportResponse>
{
    private readonly IMeetingService _meetingService;

    public MeetingSummaryPdfExportCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<MeetingRecordPdfExportResponse> Handle(IReceiveContext<MeetingRecordPdfExportCommand> context, CancellationToken cancellationToken)
    {
        return await _meetingService.MeetingRecordPdfExportAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}