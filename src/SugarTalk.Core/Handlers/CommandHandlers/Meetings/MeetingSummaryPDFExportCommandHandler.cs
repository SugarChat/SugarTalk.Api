using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class MeetingSummaryPDFExportCommandHandler : ICommandHandler<MeetingSummaryPDFExportCommand, MeetingSummaryPDFExportResponse>
{
    private readonly IMeetingService _meetingService;

    public MeetingSummaryPDFExportCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<MeetingSummaryPDFExportResponse> Handle(IReceiveContext<MeetingSummaryPDFExportCommand> context, CancellationToken cancellationToken)
    {
        return await _meetingService.MeetingSummaryPDFExportAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}