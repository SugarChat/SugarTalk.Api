using System.ComponentModel;

namespace SugarTalk.Messages.Enums.Meeting
{
    public enum MeetingUserSessionOnlineType
    {
        [Description("在线")]
        Online = 0,
        
        [Description("正常退出")]
        OutMeeting = 1,

        [Description("被踢出")]
        KickOutMeeting = 2,

        [Description("超时退出")]
        TimeOutMeeting = 3
    }
}
