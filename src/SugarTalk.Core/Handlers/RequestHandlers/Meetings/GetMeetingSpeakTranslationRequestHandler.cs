using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings;

public class GetMeetingSpeakTranslationRequestHandler : IRequestHandler<GetMeetingSpeakTranslationRequest, GetMeetingSpeakTranslationResponse>
{
    private readonly IMeetingService _meetingService;

    public GetMeetingSpeakTranslationRequestHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task<GetMeetingSpeakTranslationResponse> Handle(IReceiveContext<GetMeetingSpeakTranslationRequest> context, CancellationToken cancellationToken)
    {
        return await _meetingService.GetMeetingSpeakTranslationAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}