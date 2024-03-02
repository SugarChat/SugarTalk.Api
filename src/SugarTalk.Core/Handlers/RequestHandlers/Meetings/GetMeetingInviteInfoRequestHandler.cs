using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings;

public class GetMeetingInviteInfoRequestHandler : IRequestHandler<GetMeetingInviteInfoRequest, GetMeetingInviteInfoResponse>
{
    private readonly IMeetingService _meetingService;

    public GetMeetingInviteInfoRequestHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task<GetMeetingInviteInfoResponse> Handle(IReceiveContext<GetMeetingInviteInfoRequest> context, CancellationToken cancellationToken)
    {
        return await _meetingService.GetMeetingInviteInfoAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}