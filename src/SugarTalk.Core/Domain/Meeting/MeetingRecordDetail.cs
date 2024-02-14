using System;
using SugarTalk.Messages.Enums.Record;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.Meeting;

[Table("meeting_record_detail")]
public class MeetingRecordDetail : IEntity
{
    [Key]
    [Column("id", TypeName = "char(36)")]
    public Guid Id { get; set; }
    
    [Column("meeting_id", TypeName = "char(36)")]
    public Guid MeetingId { get; set; }
    
    [Column("meeting_sub_id", TypeName = "char(36)")]
    public Guid MeetingSubId { get; set; }
    
    [Column("meeting_record_id", TypeName = "char(36)")]
    public Guid MeetingRecordId { get; set; }
    
    [Column("meeting_number"), StringLength(128)]
    public string MeetingNumber { get; set; }
    
    [Column("user_id")]
    public int UserId { get; set; }
    
    [Column("speak_start_time")]
    public long SpeakStartTime { get; set; }
    
    [Column("speak_end_time")]
    public long SpeakEndTime { get; set; }

    [Column("speak_status")] 
    public SpeakStatus SpeakStatus { get; set; } = SpeakStatus.Speaking;
    
    [Column("speak_content")]
    public string SpeakContent { get; set; }
    
    [Column("created_date")]
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.Now;
}