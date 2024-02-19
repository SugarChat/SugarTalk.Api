using Newtonsoft.Json;
using SugarTalk.Messages.Enums.LiveKit;

namespace SugarTalk.Messages.Dto.LiveKit.Egress;

public class StartEgressBaseRequestDto
{
    public string Token{ get; set; }
    
    [JsonProperty("room_name")]
    public string RoomName { get; set; }
    
    [JsonProperty("file")]
    public EgressEncodedFileOutPutDto File { get; set; }

    [JsonProperty("preset", NullValueHandling = NullValueHandling.Ignore)]
    public EgressEncodingOptionsPreset? Preset { get; set; }
    
    [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
    public EgressEncodingOptionsDto Options { get; set; }
}

public class StartRoomCompositeEgressRequestDto : StartEgressBaseRequestDto
{
    [JsonProperty("layout")]
    public string Layout { get; set; }
    
    [JsonProperty("audio_only")]
    public bool AudioOnly { get; set; }
    
    [JsonProperty("video_only")]
    public bool VideoOnly { get; set; }
    
    [JsonProperty("custom_base_url")]
    public string CustomBaseUrl { get; set; }
}

public class StartTrackCompositeEgressRequestDto : StartEgressBaseRequestDto
{
    [JsonProperty("audio_track_id")]
    public string AudioTrackId { get; set; }
    
    [JsonProperty("video_track_id")]
    public string VideoTrackId { get; set; }
}

public class StartEgressResponseDto
{
    [JsonProperty("egress_id")]
    public string EgressId { get; set; }
    
    [JsonProperty("room_name")]
    public string RoomName { get; set; }
    
    [JsonProperty("room_id")]
    public string RoomId { get; set; }
    
    [JsonProperty("status")]
    public string Status { get; set; }
}