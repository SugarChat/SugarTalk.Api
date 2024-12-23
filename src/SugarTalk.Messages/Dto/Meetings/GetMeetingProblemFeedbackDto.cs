using System;

namespace SugarTalk.Messages.Dto.Meetings;

public class GetMeetingProblemFeedbackDto
{
    public int FeedbackId { get; set; }
    
    public string Creator { get; set; }
    
    public int Categories { get; set; }
    
    public string Description { get; set; }
    
    public DateTimeOffset LastModifiedDate { get; set; }
}