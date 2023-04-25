using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.Messages.Dtos.Meetings;

public class CreateMeetingDto
{
    [JsonProperty("roomId")]
    public string MeetingNumber { get; set; }

    [JsonProperty("mode")] 
    public string Mode { get; set; }
    
    [JsonProperty("startDate")]
    public long StartDate { get; set; }
    
    [JsonProperty("endDate")]
    public long EndDate { get; set; }
    
    [JsonProperty("roomStreamList")]
    public List<string> RoomStreamList { get; set; }

    [JsonProperty("originAdress")]
    public string OriginAdress { get; set; }
}

public class CreateMeetingResponseDto
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public MeetingType MeetingType { get; set; }

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
    public string OriginAdress { get; set; }
}