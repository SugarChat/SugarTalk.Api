using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.Account;

[Table("user_session")]
public class UserSession : IEntity
{
    public UserSession()
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
        
    [Column("room_stream_id"), StringLength(128)]
    public string RoomStreamId { get; set; }
        
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("is_muted", TypeName = "tinyint(1)")]
    public bool IsMuted { get; set; }
}