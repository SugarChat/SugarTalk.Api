using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.Account
{
    [Table("user_session")]
    public class UserSession : IEntity
    {
        public UserSession()
        {
            CreatedDate = DateTimeOffset.Now;
        }
        
        [Key]
        [Column("id", TypeName = "char(36)")]
        public Guid Id { set; get; }
        
        [Column("created_date")] 
        public DateTimeOffset CreatedDate { get; set; }
        
        [Column("meeting_session_id", TypeName = "char(36)")]
        public Guid MeetingSessionId { get; set; }
        
        [Column("connection_id"), StringLength(128)]
        public string ConnectionId { get; set; }
        
        [Column("user_id", TypeName = "char(36)")]
        public Guid UserId { get; set; }
        
        [Column("username"), StringLength(128)]
        public string UserName { set; get; }
        
        [Column("user_picture")]
        public string UserPicture { get; set; }
        
        [Column("is_muted", TypeName = "tinyint(1)")]
        public bool IsMuted { get; set; }
        
        [Column("is_sharing_screen", TypeName = "tinyint(1)")]
        public bool IsSharingScreen { get; set; }
        
        [Column("is_sharing_camera", TypeName = "tinyint(1)")]
        public bool IsSharingCamera { get; set; }
    }
}