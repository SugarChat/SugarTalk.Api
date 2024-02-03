using System.Collections.Generic;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Meetings;

public class GetAppointmentMeetingsRequest : IRequest
{
    public PageSetting PageSetting { get; set; }
}

public class GetAppointmentMeetingsResponse : SugarTalkResponse<GetAppointmentMeetingsResponseDto>
{
}

public class GetAppointmentMeetingsResponseDto
{
    public int Count { get; set; }

    public List<AppointmentMeetingDto> Records { get; set; }
}