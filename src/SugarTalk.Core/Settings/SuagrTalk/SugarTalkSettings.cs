using Microsoft.Extensions.Configuration;

namespace SugarTalk.Core.Settings.SuagrTalk;

public class SugarTalkSettings : IConfigurationSetting
{
    public SugarTalkSettings(IConfiguration configuration)
    {
        ApiKey = configuration.GetValue<string>("SugarTalk:ApiKey");
        BaseUrl = configuration.GetValue<string>("SugarTalk:BaseUrl");
        Environment = configuration.GetValue<string>("SugarTalk:Environment");
    }

    public string ApiKey { get; }

    public string BaseUrl { get; }
    
    public string Environment { get; }
}