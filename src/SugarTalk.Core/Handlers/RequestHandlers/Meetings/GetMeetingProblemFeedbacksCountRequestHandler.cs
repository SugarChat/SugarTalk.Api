using System.Threading;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using System.Threading.Tasks;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings;

public class GetMeetingProblemFeedbacksCountRequestHandler : IRequestHandler<GetMeetingProblemFeedbacksCountRequest, GetMeetingProblemFeedbacksCountResponse>
{
    private readonly IMeetingService _meetingService;
    
    public GetMeetingProblemFeedbacksCountRequestHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<GetMeetingProblemFeedbacksCountResponse> Handle(IReceiveContext<GetMeetingProblemFeedbacksCountRequest> context, CancellationToken cancellationToken)
    {
        return await _meetingService.GetMeetingProblemFeedbacksCountAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}