using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.Meeting;

[Table("meeting_user_session_stream")]
public class MeetingUserSessionStream : IEntity
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Column("user_session_id")] 
    public int UserSessionId { get; set; }
    
    [Column("room_stream_id"), StringLength(128)]
    public string RoomStreamId { get; set; }
}