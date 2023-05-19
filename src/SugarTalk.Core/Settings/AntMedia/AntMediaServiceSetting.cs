using Microsoft.Extensions.Configuration;

namespace SugarTalk.Core.Settings.AntMedia;

public class AntMediaServerSetting : IConfigurationSetting
{
    public AntMediaServerSetting(IConfiguration configuration)
    {
        BaseUrl = configuration.GetValue<string>("AntMediaServer:BaseUrl");
        
        WebSocketUrl = configuration.GetValue<string>("AntMediaServer:WebSocketUrl");
    }

    public string BaseUrl { get; set; }
    
    public string WebSocketUrl { get; set; }
}