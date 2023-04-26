using System.Collections.Generic;
using Newtonsoft.Json;

namespace SugarTalk.Messages.Dto.AntMedia;

public class ConferenceRoomDto
{
    [JsonProperty("roomId")]
    public string RoomId { get; set; }
    
    [JsonProperty("startDate")]
    public long StartDate { get; set; }
    
    [JsonProperty("endDate")]
    public long EndDate { get; set; }
    
    [JsonProperty("roomStreamList")]
    public List<string> RoomStreamList { get; set; }
    
    [JsonProperty("mode")]
    public string Mode { get; set; }
    
    [JsonProperty("originAdress")]
    public string OriginAdress { get; set; }
}

public class GetAntMediaConferenceRoomInfoResponseDto
{
    [JsonProperty("roomId")]
    public string RoomId { get; set; }

    [JsonProperty("streamDetailsMap")]
    public Dictionary<string, string> StreamDetailsMap { get; set; }

    [JsonProperty("endDate")]
    public long EndDate { get; set; }

    [JsonProperty("startDate")]
    public long StartDate { get; set; }
}
