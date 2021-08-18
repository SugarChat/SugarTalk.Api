using System;
using Kurento.NET;
using Newtonsoft.Json;
using SugarTalk.Messages.Enums;

namespace SugarTalk.Messages.Dtos.Users
{
    public class UserSessionWebRtcConnectionDto
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        
        [JsonProperty("userSessionId")]
        public Guid UserSessionId { get; set; }
        
        [JsonProperty("webRtcEndpointId")]
        public string WebRtcEndpointId { get; set; }
        
        [JsonIgnore]
        public WebRtcEndpoint WebRtcEndpoint { get; set; }
        
        [JsonProperty("webRtcPeerConnectionId")]
        public string WebRtcPeerConnectionId { get; set; }
        
        [JsonProperty("receiveWebRtcConnectionId")]
        public Guid? ReceiveWebRtcConnectionId { get; set; }
        
        [JsonProperty("mediaType")]
        public UserSessionWebRtcConnectionMediaType MediaType { get; set; }
        
        [JsonProperty("connectionType")]
        public UserSessionWebRtcConnectionType ConnectionType { get; set; }
        
        [JsonProperty("connectionStatus")]
        public UserSessionWebRtcConnectionStatus ConnectionStatus { get; set; }
    }
}