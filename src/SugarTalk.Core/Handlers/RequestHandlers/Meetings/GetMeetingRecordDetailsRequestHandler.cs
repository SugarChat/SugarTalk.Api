using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings;

public class GetMeetingRecordDetailsRequestHandler: IRequestHandler<GetMeetingRecordDetailsRequest, GetMeetingRecordDetailsResponse>
{
    private readonly IMeetingRecordDetailsService _meetingRecordDetailsService;

    public GetMeetingRecordDetailsRequestHandler(IMeetingRecordDetailsService meetingRecordDetailsService)
    {
        _meetingRecordDetailsService = meetingRecordDetailsService;
    }
    
    public async Task<GetMeetingRecordDetailsResponse> Handle(IReceiveContext<GetMeetingRecordDetailsRequest> context, CancellationToken cancellationToken)
    {
        return await _meetingRecordDetailsService.GetMeetingRecordDetailsAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}