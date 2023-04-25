using System.Collections.Generic;
using Newtonsoft.Json;

namespace SugarTalk.Messages.Dtos.AntMedia;

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

public class AntMediaBroadcastDto
{
    [JsonProperty("streamId")]
    public string StreamId { get; set; }
    
    [JsonProperty("status")]
    public string Status { get; set; }
    
    [JsonProperty("playListStatus")]
    public string PlayListStatus { get; set; }
    
    [JsonProperty("created")]
    public string Type { get; set; }
    
    [JsonProperty("publishType")]
    public string PublishType { get; set; }
    
    [JsonProperty("name")]
    public string Name { get; set; }
    
    [JsonProperty("description")]
    public string Description { get; set; }
    
    [JsonProperty("publish")]
    public bool Publish { get; set; }
    
    [JsonProperty("date")]
    public long Date { get; set; }
    
    [JsonProperty("plannedStartDate")]
    public long PlannedStartDate { get; set; }
    
    [JsonProperty("plannedEndDate")]
    public long PlannedEndDate { get; set; }
    
    [JsonProperty("duration")]
    public long Duration { get; set; }
    
    

    // "endPointList": null,
    // "playListItemList": null,
    // "publicStream": true,
    // "is360": false,
    // "listenerHookURL": null,
    // "category": null,
    // "ipAddr": null,
    // "username": null,
    // "password": null,
    // "quality": null,
    // "speed": 0,
    // "streamUrl": null,
    // "originAdress": "127.0.1.1",
    // "mp4Enabled": 0,
    // "webMEnabled": 0,
    // "expireDurationMS": 0,
    // "rtmpURL": "rtmp://51.89.116.109/Sandbox/0122cb08-d7aa-4d43-bd70-4537db099d69",
    // "zombi": false,
    // "pendingPacketSize": 0,
    // "hlsViewerCount": 0,
    // "dashViewerCount": 0,
    // "webRTCViewerCount": 0,
    // "rtmpViewerCount": 0,
    // "startTime": 0,
    // "receivedBytes": 0,
    // "bitrate": 0,
    // "userAgent": "N/A",
    // "latitude": null,
    // "longitude": null,
    // "altitude": null,
    // "mainTrackStreamId": null,
    // "subTrackStreamIds": [],
    // "absoluteStartTimeMs": 0,
    // "webRTCViewerLimit": -1,
    // "hlsViewerLimit": -1,
    // "dashViewerLimit": -1,
    // "subFolder": null,
    // "currentPlayIndex": 0,
    // "metaData": "",
    // "playlistLoopEnabled": true,
    // "updateTime": 0
    
}