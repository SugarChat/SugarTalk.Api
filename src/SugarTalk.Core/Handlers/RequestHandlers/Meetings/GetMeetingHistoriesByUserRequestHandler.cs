using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings;

public class GetMeetingHistoriesByUserRequestHandler : IRequestHandler<GetMeetingHistoriesByUserRequest, GetMeetingHistoriesByUserResponse>
{
    private readonly IMeetingService _meetingService;

    public GetMeetingHistoriesByUserRequestHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task<GetMeetingHistoriesByUserResponse> Handle(IReceiveContext<GetMeetingHistoriesByUserRequest> context, CancellationToken cancellationToken)
    {
        return await _meetingService.GetMeetingHistoriesByUserAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}