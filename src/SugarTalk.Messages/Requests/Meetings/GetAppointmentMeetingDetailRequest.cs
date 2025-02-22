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
    public Guid MeetingId { get; set; }
    
    public string MeetingNumber { get; set; }
    
    public string Title { get; set; }
    
    public long StartDate { get; set; }
    
    public long EndDate { get; set; }

    public MeetingStatus Status { get; set; }
    
    public DateTimeOffset CreatedDate { get; set; }

    public int CreatorId { get; set; }

    public string CreatorName { get; set; }

    public List<GetAppointmentMeetingDetailForParticipantDto> Type { get; set; }

    public int ParticipantCount { get; set; }
}

public class GetAppointmentMeetingDetailForParticipantDto
{
    public Guid UserId { get; set; }
    
    public string UserName { get; set; }

    public bool IsMeetingMaster { get; set; }
}