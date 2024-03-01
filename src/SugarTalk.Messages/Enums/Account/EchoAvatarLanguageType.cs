using System.ComponentModel;

namespace SugarTalk.Messages.Enums.Account;

public enum EchoAvatarLanguageType
{
    [Description("未设置")]
    None,
    
    [Description("粤语")]
    Cantonese,
    
    [Description("普通话")]
    Mandarin,
    
    [Description("英语")]
    English,
    
    [Description("韩语")]
    Korean,
    
    [Description("西班牙语")]
    Spanish
}