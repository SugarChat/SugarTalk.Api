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

    [Column("meeting_sub_id", TypeName = "char(36)")]
    public Guid? MeetingSubId { get; set; }
    
    [Column("user_id")]
    public int UserId { get; set; }
    
    [Column("creator_join_time")]
    public long CreatorJoinTime { get; set; }
    
    [Column("duration")]
    public long Duration { get; set; }
    
    [Column("is_deleted", TypeName = "tinyint(1)")]
    public bool IsDeleted { get; set; }
    
    [Column("created_date")]
    public DateTimeOffset CreatedDate { get; set; }
}