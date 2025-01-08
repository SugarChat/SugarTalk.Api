namespace SugarTalk.Messages.Dto.Meetings;

public class RecordCountDto
{
    public RecordCountDto()
    {
    }

    public RecordCountDto(string id)
    {
        Id = id;
    }
    
    public string Id { get; set; }
    
    public int Count { get; set; }
}