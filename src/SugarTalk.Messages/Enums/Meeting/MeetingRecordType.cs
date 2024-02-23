using System.ComponentModel;

namespace SugarTalk.Messages.Enums.Meeting;

public enum MeetingRecordType
{
    [Description("正在录制")]
    OnRecord = 0,
    
    [Description("结束录制")]
    EndRecord = 1
}