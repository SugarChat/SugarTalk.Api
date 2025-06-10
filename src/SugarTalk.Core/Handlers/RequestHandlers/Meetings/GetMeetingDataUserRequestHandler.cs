using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings;

public class GetMeetingDataUserRequestHandler : IRequestHandler<GetMeetingDataUserRequest, GetMeetingDataUserResponse>
{
    private readonly IMeetingService _meetingService;
    
    public GetMeetingDataUserRequestHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<GetMeetingDataUserResponse> Handle(IReceiveContext<GetMeetingDataUserRequest> context, CancellationToken cancellationToken)
    {
        return await _meetingService.GetMeetingDataUserAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}