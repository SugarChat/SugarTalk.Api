using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings;

public class GetMeetingUserSessionByUserIdRequestHandler : IRequestHandler<GetMeetingUserSessionByUserIdRequest, GetMeetingUserSessionByUserIdResponse>
{
    private readonly IMeetingService _meetingService;

    public GetMeetingUserSessionByUserIdRequestHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<GetMeetingUserSessionByUserIdResponse> Handle(IReceiveContext<GetMeetingUserSessionByUserIdRequest> context, CancellationToken cancellationToken)
    {
        return await _meetingService.GetMeetingUserSessionByUserIdAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}