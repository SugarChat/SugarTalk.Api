using Microsoft.Extensions.Configuration;

namespace SugarTalk.Core.Settings.Speech;

public class SpeechSettings : IConfigurationSetting
{
    public SpeechSettings(IConfiguration configuration)
    {
        BaseUrl = configuration.GetValue<string>("Speech:BaseUrl");

        Apikey = configuration.GetValue<string>("Speech:Apikey");
    }
    
    public string BaseUrl { get; set; }
    
    public string Apikey { get; set; }
}