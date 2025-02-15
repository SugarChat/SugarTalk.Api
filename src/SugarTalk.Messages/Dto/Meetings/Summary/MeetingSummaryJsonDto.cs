using System.Collections.Generic;
using Newtonsoft.Json;

namespace SugarTalk.Messages.Dto.Meetings.Summary;

public class MeetingSummaryJsonDto
{
    [JsonProperty("meeting_summary")] 
    public string MeetingSummaryHeadline { get; set; } = "會議摘要";
    
    [JsonProperty("abstract")] 
    public List<MeetingAbstractDto> Abstract { get; set; }

    [JsonProperty("meeting_todo")]
    public string MeetingToDo { get; set; } = "會議待辦";
    
    [JsonProperty("meeting_todo_items")]
    public List<MeetingTodoItemsDto> MeetingTodoItems { get; set; }
}

public class MeetingAbstractDto
{
    [JsonProperty("abstract_title")]
    public string AbstractTitle { get; set; }

    [JsonProperty("abstract_content")]
    public string AbstractContent { get; set; }
}

public class MeetingTodoItemsDto
{
    [JsonProperty("meeting_todo_item")]
    public string MeetingTodoItem { get; set; }
}