namespace SugarTalk.Messages.Dto.Meetings.Feedback;

public class FeedbackCountDto
{
    public FeedbackCountDto()
    {
    }
        
    public FeedbackCountDto(string id)
    {
        Id = id;
    }
        
    public string Id { get; set; }
        
    public int Count { get; set; }
}