using System;
using SugarTalk.Messages.Enums.Meeting.Summary;

namespace SugarTalk.Messages.Dto.Meetings.Summary;

public class MeetingSummaryDto
{
    public int Id { get; set; }
 
    public Guid RecordId { get; set; }
    
    public string MeetingNumber { get; set; }
    
    public string SpeakIds { get; set; }
    
    public string OriginText { get; set; }
    
    public string Summary { get; set; }
    
    public SummaryStatus Status { get; set; }
    
    public DateTimeOffset CreatedDate { get; set; }
}