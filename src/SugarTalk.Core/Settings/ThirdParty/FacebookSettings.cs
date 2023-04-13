using Microsoft.Extensions.Configuration;

namespace SugarTalk.Core.Settings.ThirdParty
{
    public class FacebookSettings : IConfigurationSetting
    {
        public FacebookSettings(IConfiguration configuration)
        {
            ClientId = configuration.GetValue<string>("FacebookSettings:ClientId");

            ClientSecret = configuration.GetValue<string>("FacebookSettings:ClientSecret");
        }

        public string ClientId { get; set; }
        
        public string ClientSecret { get; set; }
    }
}