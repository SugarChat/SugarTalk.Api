using System.ComponentModel;

namespace SugarTalk.Messages.Enums.Speech;

public enum SpeechAudioLoadStatus
{
    [Description("等待中")]
    Pending = 10,
    
    [Description("处理中")]
    Progress = 20, 
    
    [Description("已完成")]
    Completed = 30,
}