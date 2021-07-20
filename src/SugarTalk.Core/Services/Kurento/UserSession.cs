using System;
using System.Collections.Concurrent;
using Kurento.NET;
using Newtonsoft.Json;

namespace SugarTalk.Core.Services.Kurento
{
    public class UserSession
    {
        [JsonProperty("id")]
        public string Id { set; get; }
        
        [JsonProperty("userid")]
        public Guid UserId { get; set; }
        
        [JsonProperty("username")]
        public string UserName { set; get; }
        
        [JsonProperty("sendEndPoint")]
        public WebRtcEndpoint SendEndPoint { set; get; }
        
        [JsonIgnore]
        public ConcurrentDictionary<string, WebRtcEndpoint> ReceivedEndPoints { set; get; }
    }
}