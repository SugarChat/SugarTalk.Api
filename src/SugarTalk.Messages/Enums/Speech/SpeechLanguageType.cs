using System.ComponentModel;

namespace SugarTalk.Messages.Enums.Speech;

public enum SpeechTargetLanguageType
{
    [Description("广东话")]
    Cantonese = 11,
    
    [Description("普通话")]
    Mandarin = 12,
    
    [Description("英语")]
    English = 13,
    
    [Description("日本语")]
    Japanese = 14,
    
    [Description("韩语")]
    Korean = 15,
    
    [Description("西班牙语")]
    Spanish = 16,
    
    [Description("法语")]
    French = 17
}