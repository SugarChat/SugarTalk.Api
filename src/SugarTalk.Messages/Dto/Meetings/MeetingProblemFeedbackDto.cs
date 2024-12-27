using System.Collections.Generic;
using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.Messages.Dto.Meetings;

public class MeetingProblemFeedbackDto
{
    public List<MeetingCategoryType> Categories { get; set; }
    
    public string Description { get; set; }
}