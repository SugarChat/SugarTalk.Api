using Microsoft.Extensions.Configuration;

namespace SugarTalk.Core.Settings.LiveKit;

public class LiveKitServerSetting : IConfigurationSetting
{
    public LiveKitServerSetting(IConfiguration configuration)
    {
        BaseUrl = configuration.GetValue<string>("LiveKitServer:BaseUrl");
    }

    public string BaseUrl { get; set; }
}