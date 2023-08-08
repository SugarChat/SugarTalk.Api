using System.Collections.Generic;
using Newtonsoft.Json;
using SugarTalk.Messages.Enums.LiveKit;

namespace SugarTalk.Messages.Dto.LiveKit;

public class CreateLiveKitRoomDto
{
    [JsonProperty("name")]
    public string MeetingNumber { get; set; }
    
    [JsonProperty("empty_timeout")]
    public int EmptyTimeOut { get; set; }
    
    [JsonProperty("max_participants")]
    public int MaxParticipants { get; set; }
    
    [JsonProperty("metadata")]
    public string MetaData { get; set; }
    
    [JsonProperty("node_id")]
    public string NodeId { get; set; }
}

public class GetLiveKitRoomListDto
{
    [JsonProperty("names")]
    public List<string> MeetingNumbers { get; set; }
}

public class GetOrDeleteLiveKitParticipantDto
{
    [JsonProperty("room")]
    public string MeetingNumber { get; set; }
    
    [JsonProperty("identity")]
    public string Identity { get; set; }
}

public class LiveKitRoom
{
    [JsonProperty("sid")]
    public string SId { get; set; }
    
    [JsonProperty("name")]
    public string MeetingNumber { get; set; }
    
    [JsonProperty("empty_timeout")]
    public string EmptyTimeOut { get; set; }
    
    [JsonProperty("max_participants")]
    public string MaxParticipants { get; set; }
    
    [JsonProperty("creation_time")]
    public string CreateTime { get; set; }
    
    [JsonProperty("turn_password")]
    public string TurnPassword { get; set; }
    
    [JsonProperty("metadata")]
    public string MetaData { get; set; }
    
    [JsonProperty("num_participants")]
    public int NumParticipants { get; set; }
    
    [JsonProperty("active_recording")]
    public bool ActiveRecording { get; set; }
}

public class LiveKitParticipantInfo
{
    [JsonProperty("sid")]
    public string SId { get; set; }
    
    [JsonProperty("identity")]
    public string Identity { get; set; }
    
    [JsonProperty("name")]
    public string Name { get; set; }
    
    [JsonProperty("state")]
    public ParticipantInfoState State { get; set; }
    
    [JsonProperty("tracks")]
    public List<LiveKitTrackInfo> Tracks { get; set; }
    
    [JsonProperty("metadata")]
    public string MetaData { get; set; }
    
    [JsonProperty("joined_at")]
    public long JoinedAt { get; set; }
    
    [JsonProperty("permission")]
    public ParticipantPermission Permission { get; set; }
    
    [JsonProperty("is_publisher")]
    public bool IsPublisher { get; set; }
}

public class LiveKitTrackInfo
{
    [JsonProperty("sid")]
    public string SId { get; set; }
    
    [JsonProperty("type")]
    public TrackType TrackType { get; set; }
    
    [JsonProperty("source")]
    public TrackSourceType TrackSourceType { get; set; }
    
    [JsonProperty("name")]
    public string TrackName { get; set; }
    
    [JsonProperty("mime_type")]
    public string MimeType { get; set; }
    
    [JsonProperty("muted")]
    public bool Muted { get; set; }
    
    [JsonProperty("width")]
    public int Width { get; set; }
    
    [JsonProperty("height")]
    public int Height { get; set; }
    
    [JsonProperty("simulcast")]
    public bool Simulcast { get; set; }
    
    [JsonProperty("disable_dtx")]
    public bool DisableDTX { get; set; }
    
    [JsonProperty("layers")]
    public List<VideoLayer> Layers { get; set; }
}

public class VideoLayer
{
    [JsonProperty("quality")]
    public VideoQualityType VideoQualityType { get; set; }
    
    [JsonProperty("width")]
    public int Width { get; set; }
    
    [JsonProperty("height")]
    public int Height { get; set; }
}

public class ParticipantPermission
{
    [JsonProperty("can_subscribe")]
    public bool CanSubscribe { get; set; }
    
    [JsonProperty("can_publish")]
    public bool CanPublish { get; set; }
    
    [JsonProperty("can_publish_data")]
    public bool CanPublishData { get; set; }
}

