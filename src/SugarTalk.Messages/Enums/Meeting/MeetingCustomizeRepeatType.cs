using System.ComponentModel;

namespace SugarTalk.Messages.Enums.Meeting;

public enum MeetingCustomizeRepeatType
{
    [Description("自定义周")]
    CustomWeekly,
    
    [Description("自定义天")]
    CustomDaily,
    
    [Description("自定义月")]
    CustomMonthly 
}