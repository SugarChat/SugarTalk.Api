using System;
using SugarTalk.Messages.Enums.Speech;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SugarTalk.Core.Domain.Meeting;

[Table("meeting_speech_voice_table")]
public class MeetingSpeechVoiceTable : IEntity
{
    public MeetingSpeechVoiceTable()
    {
        Status = SpeechAudioLoadStatus.Pending;
        CreatedDate = DateTimeOffset.Now;
    }

    [Key]
    [Column("id")]
    public int Id { get; set; }
    
    [Column("meeting_speech_id")]
    public Guid MeetingSpeechId { get; set; }

    [Column("voice_id")]
    public string VoiceId { get; set; }

    [Column("language_id")]
    public string LanguageId { get; set; }

    [Column("translate_text")]
    public string TranslateText { get; set; }
    
    [Column("voice_url")]
    public string VoiceUrl { get; set; }

    [Column("status")]
    public SpeechAudioLoadStatus Status { get; set; }
    
    [Column("created_data")]
    public DateTimeOffset CreatedDate { get; set; }
}