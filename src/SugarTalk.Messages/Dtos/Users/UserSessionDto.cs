using System;
using Newtonsoft.Json;

namespace SugarTalk.Messages.Dtos.Users
{
    public class UserSessionDto
    {
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
    }
}