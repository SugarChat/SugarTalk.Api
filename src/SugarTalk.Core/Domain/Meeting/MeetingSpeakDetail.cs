using System;
using System.ComponentModel.DataAnnotations;
using SugarTalk.Messages.Enums.Meeting.Speak;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.Meeting;

[Table("meeting_speak_detail")]
public class MeetingSpeakDetail : IEntity
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Column("meeting_number"), StringLength(48)]
    public string MeetingNumber { get; set; }
    
    [Column("meeting_record_id", TypeName = "char(36)")]
    public Guid MeetingRecordId { get; set; }
    
    [Column("track_id")]
    public string TrackId { get; set; }
    
    [Column("user_id")]
    public int UserId { get; set; }
    
    [Column("username"), StringLength(128)]
    public string Username { get; set; }
    
    [Column("speak_start_time")]
    public long SpeakStartTime { get; set; }
    
    [Column("speak_end_time")]
    public long? SpeakEndTime { get; set; }

    [Column("speak_status")] 
    public SpeakStatus SpeakStatus { get; set; } = SpeakStatus.Speaking;
    
    [Column("original_content")]
    public string OriginalContent { get; set; }
    
    [Column("smart_content")]
    public string SmartContent { get; set; }

    [Column("file_transcription_status")] 
    public FileTranscriptionStatus FileTranscriptionStatus { get; set; } = FileTranscriptionStatus.Pending;

    [Column("created_date")]
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.Now;
}