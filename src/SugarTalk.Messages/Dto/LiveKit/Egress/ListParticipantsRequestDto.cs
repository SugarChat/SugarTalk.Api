using System.Collections.Generic;
using Newtonsoft.Json;

namespace SugarTalk.Messages.Dto.LiveKit.Egress;

public class ListParticipantsRequestDto : BaseEgressRequestDto
{
    [JsonProperty("room")]
    public string Room { get; set; }
}

public class ListParticipantsResponseDto
{
    [JsonProperty("participants")]
    public List<ParticipantDto> Participants { get; set; }
}

public class ParticipantDto
{
    [JsonProperty("sid")]
    public string SId { get; set; }

    [JsonProperty("identity")]
    public string Identity { get; set; }

    [JsonProperty("state")]
    public string State { get; set; }

    [JsonProperty("tracks")]
    public List<TrackDto> Tracks { get; set; }
    
    [JsonProperty("metaData")]
    public string MetaData { get; set; }

    [JsonProperty("joined_at")]
    public string JoinedAt { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("version")]
    public int Version { get; set; }

    [JsonProperty("permiisions")]
    public List<PermissionDto> Permissions { get; set; }

    [JsonProperty("region")]
    public string Region { get; set; }

    [JsonProperty("is_publisher")]
    public bool IsPublisher { get; set; }
}

public class PermissionDto
{
    [JsonProperty("can_subscribe")]
    public bool CanSubscribe { get; set; }

    [JsonProperty("can_publish")]
    public bool CanPublish { get; set; }

    [JsonProperty("can_publish_data")]
    public bool CanPublishData { get; set; }

    [JsonProperty("can_publish_sources")]
    public List<string> CanPublishSources { get; set; }

    [JsonProperty("hidden")]
    public bool Hidden { get; set; }

    [JsonProperty("recorder")]
    public bool Recorder { get; set; }

    [JsonProperty("can_update_matadata")]
    public bool CanUpdateMetaData { get; set; }
}

public class TrackDto
{
    [JsonProperty("sid")]
    public string SId { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("muted")]
    public bool Muted { get; set; }

    [JsonProperty("width")]
    public int Width { get; set; }

    [JsonProperty("height")]
    public int Height { get; set; }

    [JsonProperty("simulcast")]
    public bool Simulcast { get; set; }

    [JsonProperty("disable_dtx")]
    public bool DisableDtx { get; set; }

    [JsonProperty("source")]
    public string Source { get; set; }
    
    [JsonProperty("mine_type")]
    public string MineType { get; set; }

    [JsonProperty("mid")]
    public int Mid { get; set; }

    [JsonProperty("codecs")]
    public List<string> Codecs { get; set; }

    [JsonProperty("stereo")]
    public bool Stereo { get; set; }

    [JsonProperty("disable_red")]
    public bool DisableRed { get; set; }

    [JsonProperty("encryption")]
    public string Encryption { get; set; }

    [JsonProperty("stream")]
    public string Stream { get; set; }
}