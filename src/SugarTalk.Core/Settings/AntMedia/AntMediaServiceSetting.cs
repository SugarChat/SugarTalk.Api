using Microsoft.Extensions.Configuration;

namespace SugarTalk.Core.Settings.AntMedia;

public class AntMediaServiceSetting : IConfigurationSetting
{
    public AntMediaServiceSetting(IConfiguration configuration)
    {
        BaseUrl = configuration.GetValue<string>("AntMediaService:BaseUrl");
        AppName = configuration.GetValue<string>("AntMediaService:AppName");
    }

    public string BaseUrl { get; set; }
    
    public string AppName { get; set; }
}