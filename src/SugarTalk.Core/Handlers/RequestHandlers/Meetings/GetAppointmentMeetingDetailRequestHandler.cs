using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings;

public class GetAppointmentMeetingDetailRequestHandler : IRequestHandler<GetAppointmentMeetingDetailRequest, GetAppointmentMeetingDetailResponse>
{
    private readonly IMeetingService _meetingService;
    
    public GetAppointmentMeetingDetailRequestHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task<GetAppointmentMeetingDetailResponse> Handle(IReceiveContext<GetAppointmentMeetingDetailRequest> context, CancellationToken cancellationToken)
    {
        return await _meetingService.GetAppointmentMeetingsDetailAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}