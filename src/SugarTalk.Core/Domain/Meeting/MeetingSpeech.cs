using System;
using System.Collections.Generic;
using SugarTalk.Messages.Enums.Speech;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.Meeting;

[Table("meeting_speech")]
public class MeetingSpeech : IEntity
{
    public MeetingSpeech()
    {
        Status = SpeechStatus.UnViewed;
        CreatedDate = DateTimeOffset.Now;
    }

    [Key]
    [Column("id", TypeName = "char(36)")]
    public Guid Id { get; set; }

    [Column("meeting_id", TypeName = "char(36)")]
    public Guid MeetingId { get; set; }

    [Column("voice_id", TypeName = "varchar(48)")]
    public string VoiceId { get; set; }

    [Column("user_id")] 
    public int UserId { get; set; }

    [Column("original_text")] 
    public string OriginalText { get; set; }

    [Column("created_date")] 
    public DateTimeOffset CreatedDate { get; set; }

    [Column("status")]
    public SpeechStatus Status { get; set; }
    
    [NotMapped]
    public List<MeetingChatVoiceRecord> VoiceRecords { get; set; }
}