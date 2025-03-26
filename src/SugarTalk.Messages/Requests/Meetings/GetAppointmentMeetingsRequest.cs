using System;
using System.Collections.Generic;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Meetings;

public class GetAppointmentMeetingsRequest : PageSetting, IRequest
{
    public DateTimeOffset UserCurrentTime { get; set; } = DateTimeOffset.Now;
}

public class GetAppointmentMeetingsResponse : SugarTalkResponse<GetAppointmentMeetingsResponseDto>
{
}

public class GetAppointmentMeetingsResponseDto
{
    public int Count { get; set; }

    public List<AppointmentMeetingDto> Records { get; set; }
}