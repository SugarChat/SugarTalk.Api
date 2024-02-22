using System.ComponentModel;

namespace SugarTalk.Messages.Dto.Translation;

public enum TranslationLanguage
{
    [Description("zh")]
    ZhCn,
    
    [Description("en")]
    EnUs,
    
    [Description("es")]
    EsEs,
    
    [Description("ko")]
    KoKr
}