using System.ComponentModel;

namespace SugarTalk.Messages.Enums.Speech;

public enum MeetingBackLoadingStatus
{
    [Description("等待中")]
    Pending = 10,
    
    [Description("处理中")]
    Progress = 20,
    
    [Description("已完成")]
    Completed = 30,
    
    [Description("生成失败")]
    Exception = 40
}