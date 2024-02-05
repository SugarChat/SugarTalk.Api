using System;
using SugarTalk.Messages.Enums.Meeting;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.Meeting;

[Table("meeting_sub_meeting")]
public class MeetingSubMeeting : IEntity
{
    [Key]
    [Column("id", TypeName = "char(36)")]
    public Guid Id { get; set; }
    
    [Column("meeting_id", TypeName = "char(36)")]
    public Guid MeetingId { get; set; }
    
    [Column("status")]
    public MeetingRecordSubConferenceStatus SubConferenceStatus { get; set; }
    
    [Column("start_time")]
    public long StartTime { get; set; }
    
    [Column("end_time")]
    public long EndTime { get; set; }
}