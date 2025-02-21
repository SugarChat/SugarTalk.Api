using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings;

public class GetAllMeetingUserSessionsForMeetingIdRequestHandler : IRequestHandler<GetAllMeetingUserSessionsForMeetingIdRequest, GetAllMeetingUserSessionsForMeetingIdResponse>
{
    private readonly IMeetingService _meetingService;
    
    public GetAllMeetingUserSessionsForMeetingIdRequestHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<GetAllMeetingUserSessionsForMeetingIdResponse> Handle(IReceiveContext<GetAllMeetingUserSessionsForMeetingIdRequest> context, CancellationToken cancellationToken)
    {
        return await _meetingService.GetAllMeetingUserSessionByMeetingIdAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}