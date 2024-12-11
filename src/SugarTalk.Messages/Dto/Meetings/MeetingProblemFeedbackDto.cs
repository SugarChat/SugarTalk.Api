using System;
using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.Messages.Dto.Meetings;

public class MeetingProblemFeedbackDto
{
    public string Creator { get; set; }
    
    public MeetingCategoryType Category { get; set; }
    
    public string Description { get; set; }
}