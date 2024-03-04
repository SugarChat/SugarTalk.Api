using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Requests.Speech;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings.Speech;

public class GetMeetingAudioListRequestHandler : IRequestHandler<GetMeetingAudioListRequest, GetMeetingAudioListResponse>
{
    private readonly IMeetingService _meetingService;

    public GetMeetingAudioListRequestHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<GetMeetingAudioListResponse> Handle(IReceiveContext<GetMeetingAudioListRequest> context, CancellationToken cancellationToken)
    {
        return await _meetingService.GetMeetingVoiceListAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}