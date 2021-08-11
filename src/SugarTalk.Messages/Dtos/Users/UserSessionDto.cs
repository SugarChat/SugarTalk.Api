using System;
using System.Collections.Concurrent;
using Kurento.NET;
using Newtonsoft.Json;

namespace SugarTalk.Messages.Dtos.Users
{
    public class UserSessionDto
    {
        public UserSessionDto()
        {
            ReceivedEndPoints = new ConcurrentDictionary<string, WebRtcEndpoint>();
        }
        
        [JsonProperty("id")]
        public Guid Id { set; get; }
        
        [JsonProperty("createdDate")]
        public DateTimeOffset CreatedDate { get; set; }
        
        [JsonProperty("meetingSessionId")]
        public Guid MeetingSessionId { get; set; }
        
        [JsonProperty("connectionId")]
        public string ConnectionId { get; set; }
        
        [JsonProperty("userid")]
        public Guid UserId { get; set; }
        
        [JsonProperty("username")]
        public string UserName { set; get; }
        
        [JsonProperty("picture")]
        public string UserPicture { get; set; }
        
        [JsonProperty("isMuted")]
        public bool IsMuted { get; set; }
        
        [JsonProperty("isSharingScreen")]
        public bool IsSharingScreen { get; set; }
        
        [JsonProperty("isSharingCamera")]
        public bool IsSharingCamera { get; set; }
        
        [JsonProperty("webRtcEndpointId")]
        public string WebRtcEndpointId { get; set; }
        
        [JsonProperty("receivedEndPointIds")]
        public ConcurrentDictionary<string, string> ReceivedEndPointIds { get; set; }

        [JsonIgnore]
        public WebRtcEndpoint SendEndPoint { set; get; }
        
        [JsonIgnore]
        public ConcurrentDictionary<string, WebRtcEndpoint> ReceivedEndPoints { set; get; }
    }
}