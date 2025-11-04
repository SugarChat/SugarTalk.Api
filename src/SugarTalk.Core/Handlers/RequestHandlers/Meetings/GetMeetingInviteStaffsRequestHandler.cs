using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings;

public class GetMeetingInviteStaffsRequestHandler : IRequestHandler<GetMeetingInvitationUsersRequest, GetMeetingInvitationUsersResponse>
{
    private readonly IMeetingService _meetingService;

    public GetMeetingInviteStaffsRequestHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<GetMeetingInvitationUsersResponse> Handle(IReceiveContext<GetMeetingInvitationUsersRequest> context, CancellationToken cancellationToken)
    {
        return await _meetingService.GetMeetingInvitationUsersAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}