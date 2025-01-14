using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings;

public class GetMeetingRecordCountRequestHandler : IRequestHandler<GetMeetingRecordCountRequest, GetMeetingRecordCountResponse>
{
    private readonly IMeetingService _meetingService;

    public GetMeetingRecordCountRequestHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task<GetMeetingRecordCountResponse> Handle(IReceiveContext<GetMeetingRecordCountRequest> context, CancellationToken cancellationToken)
    {
        return await _meetingService.GetMeetingRecordCountAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}