using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings;

public class GetMeetingRecordEgressRequestHandler : IRequestHandler<GetMeetingRecordEgressRequest, GetMeetingRecordEgressResponse>
{
    private readonly IMeetingService _meetingService;
    
    public GetMeetingRecordEgressRequestHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<GetMeetingRecordEgressResponse> Handle(IReceiveContext<GetMeetingRecordEgressRequest> context, CancellationToken cancellationToken)
    {
        return await _meetingService.GetMeetingRecordEgressAsync(context.Message, cancellationToken).ConfigureAwait(false); 
    }
}