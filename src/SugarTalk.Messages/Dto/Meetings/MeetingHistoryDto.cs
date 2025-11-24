using System;
using System.Collections.Generic;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Messages.Dto.Meetings;

public class MeetingHistoryDto
{
    public Guid Id { get; set; }
    
    public Guid MeetingId { get; set; }
    
    public Guid? MeetingSubId { get; set; }
    
    public int UserId { get; set; }
    
    public string MeetingNumber { get; set; }
    
    public string Title { get; set; }
    
    public long StartDate { get; set; }
    
    public long EndDate { get; set; }
    
    public long Duration { get; set; }
    
    public string TimeZone { get; set; }
    
    public string MeetingCreator { get; set; }
    
    public List<AttendeesDto> Attendees { get; set; }

    public int AttendeesCount { get; set; }
    
    public MeetingAppointmentType? AppointmentType { get; set; }
}

public class AttendeesDto
{
    public int Id { get; set; }
    
    public string UserName { get; set; }

    public bool? IsHost { get; set; }
    
    public string Url { get; set; }
}