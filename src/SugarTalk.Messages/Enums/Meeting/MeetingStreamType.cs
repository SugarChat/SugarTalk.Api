using System.ComponentModel;

namespace SugarTalk.Messages.Enums.Meeting;

public enum MeetingStreamType
{
    [Description("音频")]
    Audio,
    
    [Description("屏幕共享")]
    ScreenSharing
}