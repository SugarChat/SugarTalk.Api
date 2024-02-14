using System;
using SugarTalk.Messages.Enums.Meeting.Speak;

namespace SugarTalk.Messages.Dto.Meetings.Speak;

public class MeetingSpeakDetailDto
{
    public Guid Id { get; set; }
    
    public Guid MeetingId { get; set; }
    
    public Guid? MeetingSubId { get; set; }
    
    public Guid? MeetingRecordId { get; set; }
    
    public int UserId { get; set; }
    
    public long SpeakStartTime { get; set; }
    
    public long? SpeakEndTime { get; set; }
    
    public SpeakStatus SpeakStatus { get; set; }
    
    public string SpeakContent { get; set; }
    
    public DateTimeOffset CreatedDate { get; set; }
}