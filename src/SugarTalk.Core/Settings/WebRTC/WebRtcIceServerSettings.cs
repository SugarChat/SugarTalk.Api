using Microsoft.Extensions.Configuration;

namespace SugarTalk.Core.Settings.WebRTC
{
    public class WebRtcIceServerSettings : IConfigurationSetting
    {
        public WebRtcIceServerSettings(IConfiguration configuration)
        {
            IceServers = configuration.GetValue<string>("WebRtcIceServerSettings:IceServers");
        }

        public string IceServers { get; set; }
    } 

    public class WebRtcIceServer
    {
        public string[] Urls { get; set; }
        
        public string Username { get; set; }
        
        public string Credential { get; set; }
        
        public string CredentialType { get; set; }
    }
}