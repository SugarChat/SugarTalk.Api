using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings;

public class GetAppointmentMeetingsRequestHandler : IRequestHandler<GetAppointmentMeetingsRequest, GetAppointmentMeetingsResponse>
{
    private readonly IMeetingService _meetingService;

    public GetAppointmentMeetingsRequestHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task<GetAppointmentMeetingsResponse> Handle(IReceiveContext<GetAppointmentMeetingsRequest> context, CancellationToken cancellationToken)
    {
        return await _meetingService.GetAppointmentMeetingsAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}