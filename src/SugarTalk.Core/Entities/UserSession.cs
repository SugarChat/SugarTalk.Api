using System;
using System.Collections.Concurrent;
using Kurento.NET;
using Newtonsoft.Json;

namespace SugarTalk.Core.Entities
{
    public class UserSession
    {
        [JsonProperty("id")]
        public string Id { set; get; }
        
        [JsonProperty("userid")]
        public Guid UserId { get; set; }
        
        [JsonProperty("username")]
        public string UserName { set; get; }
        
        [JsonProperty("avatar")]
        public string Avatar { get; set; }
        
        [JsonProperty("isSharingScreen")]
        public bool IsSharingScreen { get; set; }
        
        [JsonProperty("isSharingCamera")]
        public bool IsSharingCamera { get; set; }
        
        [JsonIgnore]
        public WebRtcEndpoint SendEndPoint { set; get; }
        
        [JsonIgnore]
        public ConcurrentDictionary<string, WebRtcEndpoint> ReceivedEndPoints { set; get; }
    }
}