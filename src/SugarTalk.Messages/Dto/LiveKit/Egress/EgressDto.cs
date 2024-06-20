using System.Collections.Generic;
using Newtonsoft.Json;

namespace SugarTalk.Messages.Dto.LiveKit.Egress;

public class EgressEncodedFileOutPutDto
{
    [JsonProperty("filepath")]
    public string FilePath { get; set; }
    
    [JsonProperty("file_type")] 
    public string FileType { get; set; } = "MP4";
    
    [JsonProperty("disable_manifest")]
    public bool DisableManifest { get; set; }
    
    [JsonProperty("s3", NullValueHandling = NullValueHandling.Ignore)]
    public EgressS3UploadDto S3Upload { get; set; }
    
    [JsonProperty("gcp", NullValueHandling = NullValueHandling.Ignore)]
    public EgressGcpUploadDto GcpUpload { get; set; }
    
    [JsonProperty("azure", NullValueHandling = NullValueHandling.Ignore)]
    public EgressAzureBlobUploadDto AzureBlobUpload { get; set; }
    
    [JsonProperty("aws", NullValueHandling = NullValueHandling.Ignore)]
    public EgressAwsS3UploadDto AwsS3Upload { get; set; }
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

public class EgressAwsS3UploadDto
{
    [JsonProperty("access_key")]
    public string AccessKey { get; set; }
    
    [JsonProperty("secret")]
    public string Secret { get; set; }
    
    [JsonProperty("bucket")]
    public string Bucket { get; set; }
    
    [JsonProperty("region", NullValueHandling = NullValueHandling.Ignore)]
    public string Region { get; set; }
    
    [JsonProperty("endpoint")]
    public string Endpoint { get; set; }
}

public class EgressItemDto
{
    [JsonProperty("egress_id")]
    public string EgressId { get; set; }

    [JsonProperty("room_id")]
    public string RoomId { get; set; }

    [JsonProperty("room_name")]
    public string RoomName { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("started_at")]
    public string StartedAt { get; set; }

    [JsonProperty("ended_at")]
    public string EndedAt { get; set; }

    [JsonProperty("updated_at")]
    public string UpdatedAt { get; set; }

    [JsonProperty("error")]
    public string Error { get; set; }

    [JsonProperty("room_composite")]
    public MeetingComposite MeetingComposite { get; set; }

    [JsonProperty("file")]
    public FileDetails File { get; set; }

    [JsonProperty("stream_results")]
    public List<object> StreamResults { get; set; }

    [JsonProperty("file_results")]
    public List<FileDetails> FileResults { get; set; }

    [JsonProperty("segment_results")]
    public List<object> SegmentResults { get; set; }
}

public class MeetingComposite
{
    [JsonProperty("room_name")]
    public string RoomName { get; set; }

    [JsonProperty("layout")]
    public string Layout { get; set; }

    [JsonProperty("audio_only")]
    public bool AudioOnly { get; set; }

    [JsonProperty("video_only")]
    public bool VideoOnly { get; set; }

    [JsonProperty("custom_base_url")]
    public string CustomBaseUrl { get; set; }

    [JsonProperty("file")]
    public EgressEncodedFileOutPutDto File { get; set; }

    [JsonProperty("file_outputs")]
    public List<object> FileOutputs { get; set; }

    [JsonProperty("stream_outputs")]
    public List<object> StreamOutputs { get; set; }

    [JsonProperty("segment_outputs")]
    public List<object> SegmentOutputs { get; set; }
}

public class FileDetails
{
    [JsonProperty("filename")]
    public string Filename { get; set; }

    [JsonProperty("started_at")]
    public string StartedAt { get; set; }

    [JsonProperty("ended_at")]
    public string EndedAt { get; set; }

    [JsonProperty("duration")]
    public string Duration { get; set; }

    [JsonProperty("size")]
    public string Size { get; set; }

    [JsonProperty("location")]
    public string Location { get; set; }
}

public class BaseEgressRequestDto
{
    public string Token { get; set; }
    
    [JsonProperty("egress_id")]
    public string EgressId { get; set; }
}