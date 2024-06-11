using Microsoft.Extensions.Configuration;

namespace SugarTalk.Core.Settings.Smarties;

public class SmartiesSettings : IConfigurationSetting
{
    public SmartiesSettings(IConfiguration configuration)
    {
        ApiKey = configuration.GetValue<string>("Smarties:ApiKey");
        BaseUrl = configuration.GetValue<string>("Smarties:BaseUrl");
    }
    
    public string ApiKey { get; }

    public string BaseUrl { get; }
}