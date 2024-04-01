using System.ComponentModel;

namespace SugarTalk.Messages.Enums.Meeting;

public enum MeetingStatus
{
    [Description("待开始")]
    Pending,

    [Description("进行中")]
    InProgress,

    [Description("已结束")]
    Completed,
    
    [Description("已取消")]
    Cancelled
}

public enum CurrentMeetingStatus
{
    Pending,
    
    Inprogress
}