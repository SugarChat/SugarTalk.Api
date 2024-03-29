using System;

namespace SugarTalk.Messages.Dto.Meetings.Summary;

public class MeetingSummaryBaseInfoDto
{
    public string MeetingTitle { get; set; }
    
    public string MeetingRecord { get; set; }
    
    public string MeetingAdmin { get; set; }
    
    public DateTimeOffset MeetingDate { get; set; }
}