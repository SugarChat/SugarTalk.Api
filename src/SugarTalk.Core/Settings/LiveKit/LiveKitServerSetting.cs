using Microsoft.Extensions.Configuration;

namespace SugarTalk.Core.Settings.LiveKit;

public class LiveKitServerSetting : IConfigurationSetting
{
    public LiveKitServerSetting(IConfiguration configuration)
    {
        BaseUrl = configuration.GetValue<string>("LiveKitServer:BaseUrl");

        Apikey = configuration.GetValue<string>("LiveKitServer:Apikey");

        ApiSecret = configuration.GetValue<string>("LiveKitServer:ApiSecret");
    }

    public string BaseUrl { get; set; }
    
    public string Apikey { get; set; }
    
    public string ApiSecret { get; set; }
}