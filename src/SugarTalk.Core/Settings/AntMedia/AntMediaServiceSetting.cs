using Microsoft.Extensions.Configuration;

namespace SugarTalk.Core.Settings.AntMedia;

public class AntMediaServiceSetting : IConfigurationSetting
{
    public AntMediaServiceSetting(IConfiguration configuration)
    {
        BaseUrl = configuration.GetValue<string>("AntMediaService:BaseUrl");
    }

    public string BaseUrl { get; set; }
}