using System;
using System.Collections.Generic;
using SugarTalk.Messages.Enums.Meeting;

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
    
    public List<string> attendees { get; set; }
    
    public MeetingAppointmentType? AppointmentType { get; set; }
}