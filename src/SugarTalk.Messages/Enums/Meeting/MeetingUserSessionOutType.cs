using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SugarTalk.Messages.Enums.Meeting
{
    public enum MeetingUserSessionOnlineType
    {
        [Description("正常退出")]
        OutMeeting = 0,

        [Description("被踢出")]
        KickOutMeeting = 1,

        [Description("超时退出")]
        TimeOutMeeting = 2,

        [Description("在线")]
        Online = 3
    }
}
