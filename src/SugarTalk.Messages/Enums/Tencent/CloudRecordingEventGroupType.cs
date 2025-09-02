using System.ComponentModel;

namespace SugarTalk.Messages.Enums.Tencent;

public enum CloudRecordingEventGroupType
{
    [Description("云端录制")]
    CloudRecord = 3,
    
    [Description("页面录制")]
    PageRecord = 8
}