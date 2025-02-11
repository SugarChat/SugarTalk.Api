using Microsoft.Extensions.Configuration;

namespace SugarTalk.Core.Settings.Smarties;

public class SmartiesSettings : IConfigurationSetting
{
    public SmartiesSettings(IConfiguration configuration)
    {
        ApiKey = configuration.GetValue<string>("Smarties:ApiKey");
        BaseUrl = configuration.GetValue<string>("Smarties:BaseUrl");
        SpeechMaticsUrl = configuration.GetValue<string>("Smarties:SpeechMatics:Url");
        SpeechMaticsApiKey = configuration.GetValue<string>("Smarties:SpeechMatics:Key");
    }
    
    public string ApiKey { get; }

    public string BaseUrl { get; }

    public string SpeechMaticsUrl { get; set; }
    
    public string SpeechMaticsApiKey { get; set; }
}