using System;
using SugarTalk.Messages.Enums.OpenAi;
using Microsoft.Extensions.Configuration;

namespace SugarTalk.Core.Settings.OpenAi;

public class OpenAiSettings : IConfigurationSetting
{
    public OpenAiSettings(IConfiguration configuration)
    {
        ApiKey = configuration.GetValue<string>("OpenAi:ApiKey");
        Organization = configuration.GetValue<string>("OpenAi:Organization");
        
        AzureApiKey = configuration.GetValue<string>("OpenAi:Azure:ApiKey");
        
        AzureApiVersion = configuration.GetValue<string>("OpenAi:Azure:ApiVersion");
        
        Provider = Enum.TryParse<OpenAiProvider>(configuration.GetValue<string>("OpenAi:Provider"), true, out var provider) ? provider : OpenAiProvider.OpenAi;
    }
    
    public string ApiKey { get; set; }
    
    public string Organization { get; set; }
    
    public string AzureApiKey { get; set; }
    
    public string AzureApiVersion { get; set; }
    
    public OpenAiProvider Provider { get; set; }
}