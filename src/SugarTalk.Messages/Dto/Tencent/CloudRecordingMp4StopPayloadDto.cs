using System.Collections.Generic;
using SugarTalk.Messages.Enums.Tencent;

namespace SugarTalk.Messages.Dto.Tencent;

public class CloudRecordingMp4StopPayloadDto
{
    public CloudRecordingMp4StopPayloadStatus Status { get; set; }
    
    public List<string> FileList { get; set; }
    
    public List<CloudRecordingMp4StopPayloadFileMessageDto> FileMessage { get; set; }
}

public class CloudRecordingMp4StopPayloadFileMessageDto
{
    public string FileName { get; set; }
    
    public string UserId { get; set; }
    
    public string TrackType { get; set; }
    
    public string MediaId { get; set; }
    
    public long StartTimeStamp { get; set; }
    
    public long EndTimeStamp { get; set; }
}