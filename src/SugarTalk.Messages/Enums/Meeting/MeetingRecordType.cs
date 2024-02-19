using System.ComponentModel;

namespace SugarTalk.Messages.Enums.Meeting;

public enum MeetingRecordType
{
    [Description("结束录制")]
    EndRecord = 0,
    
    [Description("正在录制")]
    OnRecord = 1
}