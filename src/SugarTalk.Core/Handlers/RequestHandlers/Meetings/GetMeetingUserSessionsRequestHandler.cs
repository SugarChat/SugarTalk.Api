using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings;

public class GetMeetingUserSessionsRequestHandler : IRequestHandler<GetMeetingUserSessionsRequest, GetMeetingUserSessionsResponse>
{
    private readonly IMeetingService _meetingService;

    public GetMeetingUserSessionsRequestHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<GetMeetingUserSessionsResponse> Handle(IReceiveContext<GetMeetingUserSessionsRequest> context, CancellationToken cancellationToken)
    {
        return await _meetingService.GetMeetingUserSessionsAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}