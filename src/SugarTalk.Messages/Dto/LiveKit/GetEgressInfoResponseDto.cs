using System.Collections.Generic;
using Newtonsoft.Json;

namespace SugarTalk.Messages.Dto.LiveKit;

public class GetEgressInfoListResponseDto
{
    [JsonProperty("items")]
    public List<EgressItemDto> EgressItems { get; set; }
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
    public List<FileResult> FileResults { get; set; }

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
    public MeetingFile File { get; set; }

    [JsonProperty("file_outputs")]
    public List<object> FileOutputs { get; set; }

    [JsonProperty("stream_outputs")]
    public List<object> StreamOutputs { get; set; }

    [JsonProperty("segment_outputs")]
    public List<object> SegmentOutputs { get; set; }
}

public class MeetingFile
{
    [JsonProperty("file_type")]
    public string FileType { get; set; }

    [JsonProperty("filepath")]
    public string Filepath { get; set; }

    [JsonProperty("disable_manifest")]
    public bool DisableManifest { get; set; }

    [JsonProperty("s3")]
    public EgressS3UploadDto S3Upload { get; set; }

    [JsonProperty("aliOSS")]
    public EgressAliOssUploadDto AliOssUpload { get; set; }
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

public class FileResult
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

public class GetEgressRequestDto : BaseEgressRequestDto
{
}