using System.Threading;
using Mediator.Net.Context;
using System.Threading.Tasks;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Handlers.RequestHandlers;

public class GetMeetingOnlineLongestDurationUserRequestHandler : IRequestHandler<GetMeetingOnlineLongestDurationUserRequest, GetMeetingOnlineLongestDurationUserResponse>
{
    private readonly IMeetingService _meetingService;

    public GetMeetingOnlineLongestDurationUserRequestHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<GetMeetingOnlineLongestDurationUserResponse> Handle(IReceiveContext<GetMeetingOnlineLongestDurationUserRequest> context, CancellationToken cancellationToken)
    {
        return await _meetingService.GetMeetingUserSessionByMeetingIdAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}