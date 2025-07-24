using System.ComponentModel;

namespace SugarTalk.Messages.Enums.Tencent;

public enum CloudRecordingVodCommitPayloadStatus
{
    [Description("正常上传至点播平台")]
    Success = 0,
    
    [Description("滞留在服务器或备份存储上")]
    StuckInServer = 1,
    
    [Description("上传任务异常")]
    UploadError = 2
}