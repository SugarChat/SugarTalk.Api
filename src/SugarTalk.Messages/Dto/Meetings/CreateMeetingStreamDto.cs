using Newtonsoft.Json;

namespace SugarTalk.Messages.Dto.Meetings;

public class CreateMeetingStreamResponseBaseDto
{
    [JsonProperty("success")]
    public bool Success { get; set; }
    
    [JsonProperty("message")]
    public string Message { get; set; }
    
    [JsonProperty("dataId")]
    public string DataId { get; set; }
    
    [JsonProperty("errorId")]
    public int ErrorId { get; set; }
}

public class CreateMeetingStreamDto
{
    [JsonProperty("streamId")]
    public string StreamId { get; set; }
    
    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("publish")]
    public bool Publish { get; set; }
}

public class CreateMeetingStreamResponseDto : CreateMeetingStreamResponseBaseDto
{
    [JsonProperty("streamId")]
    public string StreamId { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("playListStatus")]
    public string PlayListStatus { get; set; }

    [JsonProperty("type")]
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

    [JsonProperty("endPointList")]
    public List<StreamEndPoint> EndPointList { get; set; }

    [JsonProperty("playListItemList")]
    public List<StreamPlaylistItem> PlayListItemList { get; set; }

    [JsonProperty("publicStream")]
    public bool PublicStream { get; set; }

    [JsonProperty("is360")]
    public bool Is360 { get; set; }

    [JsonProperty("listenerHookURL")]
    public string ListenerHookURL { get; set; }

    [JsonProperty("category")]
    public string Category { get; set; }

    [JsonProperty("ipAddr")]
    public string IpAddr { get; set; }

    [JsonProperty("username")]
    public string Username { get; set; }

    [JsonProperty("password")]
    public string Password { get; set; }

    [JsonProperty("quality")]
    public string Quality { get; set; }

    [JsonProperty("speed")]
    public float Speed { get; set; }

    [JsonProperty("streamUrl")]
    public string StreamUrl { get; set; }

    [JsonProperty("originAdress")]
    public string OriginAddress { get; set; }

    [JsonProperty("mp4Enabled")]
    public int Mp4Enabled { get; set; }

    [JsonProperty("webMEnabled")]
    public int WebMEnabled { get; set; }

    [JsonProperty("expireDurationMS")]
    public long ExpireDurationMS { get; set; }

    [JsonProperty("rtmpURL")]
    public string RtmpURL { get; set; }

    [JsonProperty("zombi")]
    public bool Zombi { get; set; }

    [JsonProperty("pendingPacketSize")]
    public int PendingPacketSize { get; set; }

    [JsonProperty("hlsViewerCount")]
    public int HlsViewerCount { get; set; }

    [JsonProperty("dashViewerCount")]
    public int DashViewerCount { get; set; }

    [JsonProperty("webRTCViewerCount")]
    public int WebRTCViewerCount { get; set; }

    [JsonProperty("rtmpViewerCount")]
    public int RtmpViewerCount { get; set; }

    [JsonProperty("startTime")]
    public long StartTime { get; set; }

    [JsonProperty("receivedBytes")]
    public long ReceivedBytes { get; set; }

    [JsonProperty("bitrate")]
    public int Bitrate { get; set; }

    [JsonProperty("userAgent")]
    public string UserAgent { get; set; }

    [JsonProperty("latitude")]
    public string Latitude { get; set; }

    [JsonProperty("longitude")]
    public string Longitude { get; set; }

    [JsonProperty("altitude")]
    public string Altitude { get; set; }

    [JsonProperty("mainTrackStreamId")]
    public string MainTrackStreamId { get; set; }

    [JsonProperty("subTrackStreamIds")]
    public List<string> SubTrackStreamIds { get; set; }

    [JsonProperty("absoluteStartTimeMs")]
    public long AbsoluteStartTimeMs { get; set; }

    [JsonProperty("webRTCViewerLimit")]
    public int WebRTCViewerLimit { get; set; }

    [JsonProperty("hlsViewerLimit")]
    public int HLSViewerLimit { get; set; }

    [JsonProperty("dashViewerLimit")]
    public int DashViewerLimit { get; set; }

    [JsonProperty("subFolder")]
    public string SubFolder { get; set; }

    [JsonProperty("currentPlayIndex")]
    public int CurrentPlayIndex { get; set; }

    [JsonProperty("metaData")]
    public string MetaData { get; set; }

    [JsonProperty("playlistLoopEnabled")]
    public bool PlaylistLoopEnabled { get; set; }

    [JsonProperty("updateTime")]
    public long UpdateTime { get; set; }
}

public class StreamEndPoint
{
    [JsonProperty("status")]
    public string Status { get; set; }
    
    [JsonProperty("type")]
    public string Type { get; set; }
    
    [JsonProperty("rtmpUrl")]
    public string RtmpUrl { get; set; }
    
    [JsonProperty("endpointServiceId")]
    public string EndpointServiceId { get; set; }
}

public class StreamPlaylistItem
{
    [JsonProperty("type")]
    public string Type { get; set; }
    
    [JsonProperty("streamUrl")]
    public string StreamUrl { get; set; }
}
