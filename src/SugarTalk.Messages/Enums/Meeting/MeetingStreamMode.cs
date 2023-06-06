using System.ComponentModel;

namespace SugarTalk.Messages.Enums.Meeting;

public enum MeetingStreamMode
{
    [Description("MCU流模式")]
    MCU,
    
    [Description("LEGACY流模式")]
    LEGACY
}