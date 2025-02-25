using System;
using System.Collections.Generic;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Meetings;

public class GetAppointmentMeetingDetailRequest : IRequest
{
    public Guid MeetingId { get; set; }
}

public class GetAppointmentMeetingDetailResponse : SugarTalkResponse<GetAppointmentMeetingDetailDto>
{
}

public class GetAppointmentMeetingDetailDto
{
    public List<GetAppointmentMeetingDetailForParticipantDto> Participants { get; set; }
}

public class GetAppointmentMeetingDetailForParticipantDto
{
    public Guid UserId { get; set; }
    
    public string UserName { get; set; }

    public bool IsMeetingMaster { get; set; }
}