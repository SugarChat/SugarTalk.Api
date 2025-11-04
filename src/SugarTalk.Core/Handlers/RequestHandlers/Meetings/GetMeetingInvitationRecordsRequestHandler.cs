using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings;

public class GetMeetingInvitationRecordsRequestHandler : IRequestHandler<GetMeetingInvitationRecordsRequest, GetMeetingInvitationRecordsResponse>
{
    private readonly IMeetingService _meetingService;

    public GetMeetingInvitationRecordsRequestHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<GetMeetingInvitationRecordsResponse> Handle(IReceiveContext<GetMeetingInvitationRecordsRequest> context, CancellationToken cancellationToken)
    {
        return await _meetingService.GetMeetingInvitationRecordsAsync(context.Message, cancellationToken).ConfigureAwait(false); 
    }
}