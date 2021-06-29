using System.Collections.Concurrent;
using Kurento.NET;
using Newtonsoft.Json;

namespace SugarTalk.Core.Services.Kurento
{
    public class UserSession
    {
        public string Id { set; get; }
        public string DisplayName { set; get; }
        [JsonIgnore]
        public WebRtcEndpoint SendEndPoint { set; get; }
        [JsonIgnore]
        public ConcurrentDictionary<string, WebRtcEndpoint> ReceviedEndPoints { set; get; }
    }
}