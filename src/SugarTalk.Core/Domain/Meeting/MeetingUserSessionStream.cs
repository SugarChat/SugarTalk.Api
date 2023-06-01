using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SugarTalk.Messages.Enums.Meeting;

namespace SugarTalk.Core.Domain.Meeting;

[Table("meeting_user_session_stream")]
public class MeetingUserSessionStream : IEntity
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Column("meeting_user_session_id")]
    public int MeetingUserSessionId { get; set; }
    
    [Column("stream_id"), StringLength(128)]
    public string StreamId { get; set; }
    
    [Column("stream_type")]
    public MeetingStreamType StreamType { get; set; }
}