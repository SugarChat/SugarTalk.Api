using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings;

public class MeetingSummaryPdfExportRequestHandler : IRequestHandler<MeetingSummaryPDFExportRequest, MeetingSummaryPDFExportResponse>
{
    private readonly IMeetingService _meetingService;

    public MeetingSummaryPdfExportRequestHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<MeetingSummaryPDFExportResponse> Handle(IReceiveContext<MeetingSummaryPDFExportRequest> context, CancellationToken cancellationToken)
    {
        return await _meetingService.MeetingSummaryPdfExportAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}