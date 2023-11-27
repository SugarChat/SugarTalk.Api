using System;
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

    [Column("user_id")] 
    public int UserId { get; set; }

    [Column("original_text")] 
    public string OriginalText { get; set; }

    [Column("translated_text")] 
    public string TranslatedText { get; set; }

    [Column("created_date")] 
    public DateTimeOffset CreatedDate { get; set; }

    [Column("status")]
    public SpeechStatus Status { get; set; }
}