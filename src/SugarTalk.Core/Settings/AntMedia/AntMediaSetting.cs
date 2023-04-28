using Microsoft.Extensions.Configuration;

namespace SugarTalk.Core.Settings.AntMedia;

public class AntMediaSetting : IConfigurationSetting
{
    public AntMediaSetting(IConfiguration configuration)
    {
        BroadcastUrl = configuration.GetValue<string>("AntMedia:BroadcastUrl");
    }

    public string BroadcastUrl { get; set; }
}