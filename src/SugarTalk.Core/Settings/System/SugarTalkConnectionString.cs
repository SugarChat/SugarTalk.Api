using Microsoft.Extensions.Configuration;

namespace SugarTalk.Core.Settings.System;

public class SugarTalkConnectionString : IConfigurationSetting<string>
{
    public SugarTalkConnectionString(IConfiguration configuration)
    {
        Value = configuration.GetConnectionString("SugarTalkConnectionString");
    }
    
    public string Value { get; set; }
}