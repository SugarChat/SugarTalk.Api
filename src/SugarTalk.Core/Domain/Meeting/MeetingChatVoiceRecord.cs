using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SugarTalk.Messages.Enums.Speech;

namespace SugarTalk.Core.Domain.Meeting;

[Table("meeting_chat_voice_record")]
public class MeetingChatVoiceRecord : IEntity
{
    public MeetingChatVoiceRecord()
    {
        CreatedDate = DateTimeOffset.Now;
        VoiceLanguage = SpeechTargetLanguageType.Cantonese;
        GenerationStatus = ChatRecordGenerationStatus.InProgress;
    }
    
    [Key]
    [Column("id", TypeName = "char(36)")]
    public Guid Id { get; set; }

    [Column("voice_language")]
    public SpeechTargetLanguageType VoiceLanguage { get; set; }
    
    [Column("created_date")]
    public DateTimeOffset CreatedDate { get; set; }
    
    [Column("speech_id")]
    public Guid SpeechId { get; set; }
    
    [Column("voice_id")]
    public string VoiceId { get; set; }
    
    [Column("is_system_voice", TypeName = "tinyint(1)")]
    public bool IsSystemVoice { get; set; }
    
    [Column("voice_url")]
    public string VoiceUrl { get; set; }
    
    [Column("is_self", TypeName = "tinyint(1)")]
    public bool IsSelf { get; set; }
    
    [Column("generation_status")]
    public ChatRecordGenerationStatus GenerationStatus { get; set; }
    
    [Column("translated_text")]
    public string TranslatedText { get; set; }
}