using System.Collections.Generic;
using Newtonsoft.Json;

namespace SugarTalk.Messages.Dto.Meetings;

public class GetMeetingDto
{
    [JsonProperty("roomId")]
    public string MeetingNumber { get; set; }
}

public class GetMeetingResponseDto
{
    [JsonProperty("roomId")]
    public string MeetingNumber { get; set; }
    
    [JsonProperty("startDate")]
    public long StartDate { get; set; }
    
    [JsonProperty("endDate")]
    public long EndDate { get; set; }
    
    [JsonProperty("roomStreamList")]
    public List<string> RoomStreamList { get; set; }
    
    [JsonProperty("mode")]
    public string Mode { get; set; }
    
    [JsonProperty("originAdress")]
    public string OriginAddress { get; set; }
}