using System.Collections.Generic;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Meetings;

public class GetAppointmentMeetingRequest : IRequest
{
    public int UserId { get; set; }
}

public class GetAppointmentMeetingResponse : SugarTalkResponse<List<AppointmentMeetingDto>>
{
}