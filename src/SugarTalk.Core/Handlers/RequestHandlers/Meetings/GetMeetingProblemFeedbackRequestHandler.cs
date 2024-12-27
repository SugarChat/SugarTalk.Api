using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings;

public class GetMeetingProblemFeedbackRequestHandler : IRequestHandler<GetMeetingProblemFeedbackRequest, GetMeetingProblemFeedbackResponse>
{
    private readonly IMeetingService _meetingService;

    public GetMeetingProblemFeedbackRequestHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<GetMeetingProblemFeedbackResponse> Handle(IReceiveContext<GetMeetingProblemFeedbackRequest> context, CancellationToken cancellationToken)
    {
        return await _meetingService.GetMeetingProblemFeedbackAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}