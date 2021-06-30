using System.Collections.Concurrent;
using Kurento.NET;
using Newtonsoft.Json;

namespace SugarTalk.Core.Services.Kurento
{
    public class UserSession
    {
        [JsonProperty("id")]
        public string Id { set; get; }
        
        [JsonProperty("username")]
        public string UserName { set; get; }
        [JsonIgnore]
        public WebRtcEndpoint SendEndPoint { set; get; }
        [JsonIgnore]
        public ConcurrentDictionary<string, WebRtcEndpoint> ReceviedEndPoints { set; get; }
    }
}