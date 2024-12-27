using System.ComponentModel;

namespace SugarTalk.Messages.Enums.Meeting;

public enum MeetingCategoryType
{
    [Description("功能建議")]
    Suggestions = 0,

    [Description("功能缺陷")]
    Defect = 1,
    
    [Description("功能資訊")]
    Information = 2,
    
    [Description("其他")]
    Other = 3
}