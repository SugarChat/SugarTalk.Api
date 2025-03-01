using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings;

public class GetStaffsTreeRequestHandler : IRequestHandler<GetStaffsTreeRequest, GetStaffsTreeResponse>
{
    private readonly IMeetingService _meetingService;

    public GetStaffsTreeRequestHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task<GetStaffsTreeResponse> Handle(IReceiveContext<GetStaffsTreeRequest> context, CancellationToken cancellationToken)
    {
        return await _meetingService.GetStaffsTreeAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}