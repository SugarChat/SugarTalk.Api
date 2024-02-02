using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings;

public class GetMeetingListRequestHandler : IRequestHandler<GetAppointmentMeetingRequest, GetAppointmentMeetingResponse>
{
    private readonly IMeetingService _meetingService;

    public GetMeetingListRequestHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task<GetAppointmentMeetingResponse> Handle(IReceiveContext<GetAppointmentMeetingRequest> context, CancellationToken cancellationToken)
    {
        return await _meetingService.GetAppointmentMeetingAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}