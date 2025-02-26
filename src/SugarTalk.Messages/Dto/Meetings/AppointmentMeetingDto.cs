using System;
using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.Messages.Dto.Meetings;

public class AppointmentMeetingDto
{
    public Guid MeetingId { get; set; }
    
    public string MeetingNumber { get; set; }
    
    public string Title { get; set; }
    
    public MeetingAppointmentType AppointmentType { get; set; }

    public int Creator { get; set; }
    
    public long StartDate { get; set; }
    
    public long EndDate { get; set; }

    public MeetingStatus Status { get; set; }
    
    public DateTimeOffset CreatedDate { get; set; }
}