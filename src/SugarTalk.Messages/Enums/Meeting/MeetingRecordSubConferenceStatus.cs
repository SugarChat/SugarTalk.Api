using System.ComponentModel;

namespace SugarTalk.Messages.Enums.Meeting;

public enum MeetingRecordSubConferenceStatus
{
    [Description("默认(存在)")]
    Default = 0,

    [Description("已删除")]
    NotExist = 1
}