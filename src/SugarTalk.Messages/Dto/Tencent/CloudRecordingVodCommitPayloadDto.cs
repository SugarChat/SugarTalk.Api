using SugarTalk.Messages.Enums.Tencent;

namespace SugarTalk.Messages.Dto.Tencent;

public class CloudRecordingVodCommitPayloadDto
{
    public CloudRecordingVodCommitPayloadStatus Status { get; set; }
    
    public string? UserId { get; set; }
    
    public string TrackType { get; set; }

    public string MediaId { get; set; }
    
    public string FileId { get; set; }
    
    public string VideoUrl { get; set; }
    
    public string CacheFile { get; set; }
    
    public long StartTimeStamp { get; set; }
    
    public long EndTimeStamp { get; set; }
    
    public string Errmsg { get; set; }
}