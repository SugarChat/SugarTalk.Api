using System.ComponentModel;

namespace SugarTalk.Messages.Enums.Meeting;

public enum MeetingAttendeeStatus
{
    [Description("已出席")]
    Present = 1,
    
    [Description("未出席")]
    Absent = 2
}