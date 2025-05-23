using System.ComponentModel;

namespace SugarTalk.Messages.Enums.Meeting;

public enum MeetingRepeatType
{
    [Description("不重复")]
    None,

    [Description("每天")]
    Daily,

    [Description("每个工作日")]
    EveryWeekday,

    [Description("每周")]
    Weekly,

    [Description("每两周")]
    BiWeekly,

    [Description("每月")]
    Monthly,
    
    [Description("自定义周")]
    CustomWeekly,
    
    [Description("自定义天")]
    CustomDaily,
    
    [Description("自定义月")]
    CustomMonthly 
}