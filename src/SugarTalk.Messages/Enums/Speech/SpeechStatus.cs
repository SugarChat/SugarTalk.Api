using System.ComponentModel;

namespace SugarTalk.Messages.Enums.Speech;

public enum SpeechStatus
{
    [Description("未读")]
    UnViewed,
    
    [Description("已读")]
    Viewed,
    
    [Description("已撤回")]
    Cancelled
}