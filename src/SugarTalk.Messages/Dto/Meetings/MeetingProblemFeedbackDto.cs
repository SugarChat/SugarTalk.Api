using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.Messages.Dto.Meetings;

public class MeetingProblemFeedbackDto
{
    public MeetingCategoryType Category { get; set; }
    
    public string Description { get; set; }

    public bool IsNew { get; set; } = true;
    
    public int CreatedBy{ get; set; }
}