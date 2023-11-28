using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Requests.Meetings.User;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings.User;

public class GetMeetingUserSettingRequestHandler : IRequestHandler<GetMeetingUserSettingRequest, GetMeetingUserSettingResponse>
{
    private readonly IMeetingService _meetingService;

    public GetMeetingUserSettingRequestHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<GetMeetingUserSettingResponse> Handle(IReceiveContext<GetMeetingUserSettingRequest> context, CancellationToken cancellationToken)
    {
        return await _meetingService.GetMeetingUserSettingAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}