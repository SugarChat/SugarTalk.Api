using System;

namespace SugarTalk.Messages.Dto.Meetings.Summary;

public class MeetingSummaryBaseInfoDto
{
    public int MeetingSummaryId { get; set; }
    
    public string MeetingNumber { get; set; }
    
    public string MeetingTitle { get; set; }
    
    public string MeetingRecord { get; set; }
    
    public string MeetingAdmin { get; set; }
    
    public string Attendees { get; set; }
    
    public DateTimeOffset MeetingDate { get; set; }
}