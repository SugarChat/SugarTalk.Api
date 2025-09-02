using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings;

public class GetMeetingDataRequestHandler : IRequestHandler<GetMeetingDataRequest, GetMeetingDataResponse>
{
    private readonly IMeetingService _meetingService;
    
    public GetMeetingDataRequestHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<GetMeetingDataResponse> Handle(IReceiveContext<GetMeetingDataRequest> context, CancellationToken cancellationToken)
    {
        return await _meetingService.GetMeetingDataAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}