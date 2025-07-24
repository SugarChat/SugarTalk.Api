using System.ComponentModel;

namespace SugarTalk.Messages.Enums.Tencent;

public enum CloudRecordingMp4StopPayloadStatus
{
    [Description("录制 MP4 任务正常退出")]
    Success = 0,
    
    [Description("滞留在服务器或备份存储上")]
    StuckInServer = 1,
    
    [Description("录制 MP4 任务异常退出")]
    UploadError = 2
}