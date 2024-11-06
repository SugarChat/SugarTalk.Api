using Microsoft.Extensions.Configuration;

namespace SugarTalk.Core.Settings.PostBoy;

public class PostBoySettings : IConfigurationSetting
{
    public PostBoySettings(IConfiguration configuration)
    {
        ApiKey = configuration.GetValue<string>("PostBoy:ApiKey");
        BaseUrl = configuration.GetValue<string>("PostBoy:BaseUrl");
    }

    public string ApiKey { get; }

    public string BaseUrl { get; }
}