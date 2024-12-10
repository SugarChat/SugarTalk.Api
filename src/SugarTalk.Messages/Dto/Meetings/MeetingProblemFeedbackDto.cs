using System;
using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.Messages.Dto.Meetings;

public class MeetingProblemFeedbackDto
{
    public int? FeedbackId { get; set; }
    
    public string Creator { get; set; }
    
    public MeetingCategoryType Category { get; set; }
    
    public string Description { get; set; }
    
    public int CreatedBy { get; set; }

    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.Now;

    public DateTimeOffset LastModifiedDate { get; set; } 
}