using System;
using SugarTalk.Messages.Enums;

namespace SugarTalk.Core.Entities
{
    public class UserSessionWebRtcConnection : IEntity
    {
        public UserSessionWebRtcConnection()
        {
            ConnectionStatus = UserSessionWebRtcConnectionStatus.Connecting;
        }
        
        public Guid Id { get; set; }
        
        public Guid UserSessionId { get; set; }
        
        public string WebRtcPeerConnectionId { get; set; }
        
        public string WebRtcPeerConnectionOfferSdp { get; set; }
        
        public Guid? ReceiveWebRtcConnectionId { get; set; }
        
        public UserSessionWebRtcConnectionMediaType MediaType { get; set; }
        
        public UserSessionWebRtcConnectionStatus ConnectionStatus { get; set; }
    }
}