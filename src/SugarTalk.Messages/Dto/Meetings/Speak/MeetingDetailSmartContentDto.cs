using Newtonsoft.Json;

namespace SugarTalk.Messages.Dto.Meetings.Speak;

public class MeetingDetailSmartContentDto
{
    [JsonProperty("optimized_text")]
    public string OptimizedText { get; set; }
}