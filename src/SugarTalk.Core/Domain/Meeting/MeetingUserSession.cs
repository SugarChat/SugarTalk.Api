using System;
using SugarTalk.Messages.Enums.Meeting;
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
    
    [Column("meeting_id", TypeName = "char(36)")]
    public Guid MeetingId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }
    
    [Column("status")]
    public MeetingAttendeeStatus Status { get; set; }
    
    [Column("first_join_time")]
    public long? FirstJoinTime { get; set; }
    
    [Column("last_quit_time")]
    public long? LastQuitTime { get; set; }
    
    [Column("total_join_count")]
    public int TotalJoinCount { get; set; }
    
    [Column("cumulative_time")]
    public long? CumulativeTime { get; set; }

    [Column("is_muted", TypeName = "tinyint(1)")]
    public bool IsMuted { get; set; }
    
    [Column("is_sharing_screen", TypeName = "tinyint(1)")]
    public bool IsSharingScreen { get; set; }
    
    [Column("is_deleted", TypeName = "tinyint(1)")]
    public bool IsDeleted { get; set; }
    
    [Column("created_date")] 
    public DateTimeOffset CreatedDate { get; set; }
}