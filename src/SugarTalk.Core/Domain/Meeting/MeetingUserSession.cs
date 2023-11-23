using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.Meeting;

[Table("meeting_user_session")]
public class MeetingUserSession : IEntity
{
    public MeetingUserSession()
    {
        CreatedDate = DateTimeOffset.Now;
    }
        
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
        
    [Column("created_date")] 
    public DateTimeOffset CreatedDate { get; set; }
        
    [Column("meeting_id", TypeName = "char(36)")]
    public Guid MeetingId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("is_muted", TypeName = "tinyint(1)")]
    public bool IsMuted { get; set; }
    
    [Column("is_sharing_screen", TypeName = "tinyint(1)")]
    public bool IsSharingScreen { get; set; }
    
    [Column("name"), StringLength(128)]
    public string Name { get; set; }
    
    [Column("picture_url"), StringLength(512)]
    public string PictureUrl { get; set; }
}