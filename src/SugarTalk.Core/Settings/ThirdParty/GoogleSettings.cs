using Microsoft.Extensions.Configuration;

namespace SugarTalk.Core.Settings.ThirdParty
{
    public class GoogleSettings : IConfigurationSetting
    {
        public GoogleSettings(IConfiguration configuration)
        {
            ClientId = configuration.GetValue<string>("GoogleSettings:ClientId");
            
            ClientSecret = configuration.GetValue<string>("GoogleSettings:ClientSecret");
        }

        public string ClientId { get; set; }
        
        public string ClientSecret { get; set; }
    }
}