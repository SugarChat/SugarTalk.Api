using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings;

public class GetMeetingParticipantsUtcRequestHandler : IRequestHandler<GetMeetingParticipantsRequest, GetMeetingParticipantsResponse>
{
    private readonly IMeetingService _meetingService;

    public GetMeetingParticipantsUtcRequestHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task<GetMeetingParticipantsResponse> Handle(IReceiveContext<GetMeetingParticipantsRequest> context, CancellationToken cancellationToken)
    {
        return await _meetingService.GetMeetingParticipantsAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
