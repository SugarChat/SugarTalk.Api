using System;

namespace SugarTalk.Core.Entities
{
    public class UserSession : IEntity
    {
        public UserSession()
        {
            CreatedDate = DateTimeOffset.Now;
        }
        
        public Guid Id { set; get; }
        
        public DateTimeOffset CreatedDate { get; set; }
        
        public Guid MeetingSessionId { get; set; }
        
        public string ConnectionId { get; set; }
        
        public Guid UserId { get; set; }
        
        public string UserName { set; get; }
        
        public string UserPicture { get; set; }
        
        public bool IsMuted { get; set; }
        
        public bool IsSharingScreen { get; set; }
        
        public bool IsSharingCamera { get; set; }
    }
}