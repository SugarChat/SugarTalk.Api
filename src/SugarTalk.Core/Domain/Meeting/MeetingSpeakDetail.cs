using System;
using SugarTalk.Messages.Enums.Speak;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.Meeting;

[Table("meeting_speak_detail")]
public class MeetingSpeakDetail : IEntity
{
    [Key]
    [Column("id", TypeName = "char(36)")]
    public Guid Id { get; set; }
    
    [Column("meeting_record_id", TypeName = "char(36)")]
    public Guid MeetingRecordId { get; set; }
    
    [Column("user_id")]
    public int UserId { get; set; }
    
    [Column("speak_start_time")]
    public long SpeakStartTime { get; set; }
    
    [Column("speak_end_time")]
    public long? SpeakEndTime { get; set; }

    [Column("speak_status")] 
    public SpeakStatus SpeakStatus { get; set; } = SpeakStatus.Speaking;
    
    [Column("speak_content")]
    public string SpeakContent { get; set; }
    
    [Column("created_date")]
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.Now;
}