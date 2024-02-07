using System.ComponentModel;

namespace SugarTalk.Messages.Enums.OpenAi;

public enum TranscriptionLanguage
{
    [Description("zh")]
    Chinese,
    
    [Description("en")]
    English,
    
    [Description("es")]
    Spanish
}