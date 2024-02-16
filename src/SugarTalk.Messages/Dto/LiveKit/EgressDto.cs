using Newtonsoft.Json;
using System.Collections.Generic;
using SugarTalk.Messages.Enums.LiveKit;

namespace SugarTalk.Messages.Dto.LiveKit;

public class StartEgressBaseRequestDto
{
    public string Token{ get; set; }
    
    [JsonProperty("room_name")]
    public string RoomName { get; set; }
    
    [JsonProperty("files")]
    public EgressEncodedFileOutPutDto Files { get; set; }

    [JsonProperty("preset")]
    public EgressEncodingOptionsPreset? Preset { get; set; }
    
    [JsonProperty("options")]
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

public class StopEgressRequestDto
{
    public string Token { get; set; }
    
    [JsonProperty("egress_id")]
    public string EgressId { get; set; }
}

public class EgressEncodedFileOutPutDto
{
    [JsonProperty("filepath")]
    public string Filepath { get; set; }
    
    [JsonProperty("s3")]
    public EgressS3UploadDto S3Upload { get; set; }
    
    [JsonProperty("gcp")]
    public EgressGcpUploadDto GcpUpload { get; set; }
    
    [JsonProperty("azure")]
    public EgressAzureBlobUploadDto AzureBlobUpload { get; set; }
    
    [JsonProperty("aliOSS")]
    public EgressAliOssUploadDto AliOssUpload { get; set; }
}

public class EgressEncodingOptionsDto
{
    [JsonProperty("width")]
    public int Width { get; set; }

    [JsonProperty("height")]
    public int Height { get; set; }
    
    [JsonProperty("depth")]
    public int Depth { get; set; } = 24;
    
    [JsonProperty("framerate")]
    public int Framerate { get; set; } = 30;
    
    [JsonProperty("audio_codec")]
    public string AudioCodec { get; set; } = "AAC";
    
    [JsonProperty("audio_bitrate")]
    public int AudioBitrate { get; set; } = 128;
    
    [JsonProperty("audio_frequency")]
    public int AudioFrequency { get; set; } = 44100;
    
    [JsonProperty("video_codec")]
    public string VideoCodec { get; set; } = "H264_MAIN";
    
    [JsonProperty("video_bitrate")]
    public int VideoBitrate { get; set; } = 4500;
    
    [JsonProperty("key_frame_interval")]
    public int KeyFrameInterval { get; set; } = 4;
}

public class EgressS3UploadDto
{
    [JsonProperty("access_key")]
    public string AccessKey { get; set; }
    
    [JsonProperty("secret")]
    public string Secret { get; set; }
    
    [JsonProperty("bucket")]
    public string Bucket { get; set; }
    
    [JsonProperty("region")]
    public string Region { get; set; }
    
    [JsonProperty("endpoint")]
    public string Endpoint { get; set; }

    [JsonProperty("force_path_style")]
    public bool ForcePathStyle { get; set; }
    
    [JsonProperty("metadata")]
    public Dictionary<string, string> Metadata { get; set; }
    
    [JsonProperty("tagging")]
    public string Tagging { get; set; }
}

public class EgressGcpUploadDto
{
    [JsonProperty("credentials")]
    public string Credentials { get; set; }
    
    [JsonProperty("bucket")]
    public string Bucket { get; set; }
}

public class EgressAzureBlobUploadDto
{
    [JsonProperty("account_name")]
    public string AccountName { get; set; }
    
    [JsonProperty("account_key")]
    public string AccountKey { get; set; }
    
    [JsonProperty("container_name")]
    public string ContainerName { get; set; }
}

public class EgressAliOssUploadDto
{
    [JsonProperty("access_key")]
    public string AccessKey { get; set; }
    
    [JsonProperty("secret")]
    public string Secret { get; set; }
    
    [JsonProperty("bucket")]
    public string Bucket { get; set; }
    
    [JsonProperty("region")]
    public string Region { get; set; }
    
    [JsonProperty("endpoint")]
    public string Endpoint { get; set; }
}