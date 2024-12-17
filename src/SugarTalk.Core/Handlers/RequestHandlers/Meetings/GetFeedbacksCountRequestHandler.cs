using System.Threading;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using System.Threading.Tasks;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings;

public class GetFeedbacksCountRequestHandler : IRequestHandler<GetFeedbacksCountRequest, GetFeedbacksCountResponse>
{
    private readonly IMeetingService _meetingService;
    
    public GetFeedbacksCountRequestHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<GetFeedbacksCountResponse> Handle(IReceiveContext<GetFeedbacksCountRequest> context, CancellationToken cancellationToken)
    {
        return await _meetingService.GetFeedbacksCountAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}