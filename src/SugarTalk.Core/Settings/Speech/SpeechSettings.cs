using Microsoft.Extensions.Configuration;

namespace SugarTalk.Core.Settings.Speech;

public class SpeechSettings : IConfigurationSetting
{
    public EchoAvatarSettings EchoAvatar { get; set; }
    public SpeechSettings(IConfiguration configuration)
    {
        EchoAvatar = new EchoAvatarSettings
        {
            BaseUrl = configuration.GetValue<string>("Speech:EchoAvatar:BaseUrl"),
            Apikey = configuration.GetValue<string>("Speech:EchoAvatar:Apikey"),
            CallBackUrl = configuration.GetValue<string>("Speech:EchoAvatar:CallBackUrl"),
            CantonBaseUrl = configuration.GetValue<string>("Speech:EchoAvatar:CantonBaseUrl")
        };
        BaseUrl = configuration.GetValue<string>("Speech:BaseUrl");

        Apikey = configuration.GetValue<string>("Speech:Apikey");
    }
    
    public string BaseUrl { get; set; }
    
    public string Apikey { get; set; }
}
public class EchoAvatarSettings
{
    public string BaseUrl { get; set; }
    public string Apikey { get; set; }
    public string CallBackUrl { get; set; }
    public string CantonBaseUrl { get; set; }
}