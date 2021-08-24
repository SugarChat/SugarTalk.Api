namespace SugarTalk.Core.Settings
{
    public class WebRtcIceServerSettings
    {
        public string IceServers { get; set; }
    } 

    public class WebRtcIceServer
    {
        public string[] Urls { get; set; }
        
        public string UserName { get; set; }
        
        public string Credential { get; set; }
        
        public string CredentialType { get; set; }
    }
}