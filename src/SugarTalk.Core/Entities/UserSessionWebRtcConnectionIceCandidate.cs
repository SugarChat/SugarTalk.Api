using System;

namespace SugarTalk.Core.Entities
{
    public class UserSessionWebRtcConnectionIceCandidate : IEntity
    {
        public Guid Id { get; set; }
        
        public string WebRtcPeerConnectionId { get; set; }
        
        public string Candidate { get; set; }
        
        public string SdpMid { get; set; }
        
        public int SdpMLineIndex { get; set; }
    }
}