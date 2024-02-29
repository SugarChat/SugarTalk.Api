using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Requests.Speech;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings.Speech;

public class GetMeetingVoiceListRequestHandler : IRequestHandler<GetMeetingVoiceListRequest, GetMeetingVoiceListResponse>
{
    private readonly IMeetingService _meetingService;

    public GetMeetingVoiceListRequestHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task<GetMeetingVoiceListResponse> Handle(IReceiveContext<GetMeetingVoiceListRequest> context, CancellationToken cancellationToken)
    {
        return await _meetingService.GetMeetingVoiceListAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}