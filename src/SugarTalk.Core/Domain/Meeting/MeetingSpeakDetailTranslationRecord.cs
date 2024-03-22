using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SugarTalk.Messages.Enums.Speech;
using SugarTalk.Messages.Dto.Translation;

namespace SugarTalk.Core.Domain.Meeting;

[Table("meeting_speak_detail_translation_record")]
public class MeetingSpeakDetailTranslationRecord : IEntity
{
    public MeetingSpeakDetailTranslationRecord()
    {
        SmartTranslationContent = null;
        CreatedDate = DateTimeOffset.Now;
        OriginalTranslationContent = null;
        Status = MeetingSpeakTranslatingStatus.Pending;
    }

    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("meeting_record_id")]
    public Guid MeetingRecordId { get; set; }

    [Column("meeting_speak_detail_id")]
    public int MeetingSpeakDetailId { get; set; }

    [Column("status")]
    public MeetingSpeakTranslatingStatus Status { get; set; }

    [Column("language")]
    public TranslationLanguage Language{ get; set; }

    [Column("original_translation_content")]
    public string OriginalTranslationContent { get; set; }

    [Column("smart_translation_content")]
    public string SmartTranslationContent { get; set; }

    [Column("created_date")]
    public DateTimeOffset CreatedDate { get; set; }
}