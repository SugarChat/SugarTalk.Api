using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings;

public class GetSimpleMeetingRequestHandler : IRequestHandler<GetSimpleMeetingRequest, GetSimpleMeetingResponse>
{
    private readonly IMeetingService _meetingService;

    public GetSimpleMeetingRequestHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task<GetSimpleMeetingResponse> Handle(IReceiveContext<GetSimpleMeetingRequest> context, CancellationToken cancellationToken)
    {
        return await _meetingService.GetSimpleMeetingAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}