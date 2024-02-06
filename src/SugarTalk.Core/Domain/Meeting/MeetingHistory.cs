using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.Meeting;

[Table("meeting_history")]
public class MeetingHistory : IEntity
{
    public MeetingHistory()
    {
        CreatedDate = DateTimeOffset.Now;
    }

    [Column("id", TypeName = "char(36)")]
    public Guid Id { get; set; }
    
    [Column("meeting_id", TypeName = "char(36)")]
    public Guid MeetingId { get; set; }
    
    [Column("user_entity_id", TypeName = "char(36)")]
    public Guid UserEntityId { get; set; }
    
    [Column("creator_join_time")]
    public long CreatorJoinTime { get; set; }
    
    [Column("duration")]
    public long Duration { get; set; }
    
    [Column("is_deleted", TypeName = "tinyint(1)")]
    public bool IsDeleted { get; set; }
    
    [Column("created_date")]
    public DateTimeOffset CreatedDate { get; set; }
}