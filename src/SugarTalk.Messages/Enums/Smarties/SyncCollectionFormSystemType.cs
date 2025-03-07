using System.ComponentModel;

namespace SugarTalk.Messages.Enums.Smarties;

public enum SyncCollectionFormSystemType
{
    [Description("企业微信")]
    WorkWechat,
    
    [Description("Teambition")]
    Teambition,
    
    [Description("Busybee")]
    Busybee
}