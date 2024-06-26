using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SugarTalk.Messages.Enums.Speech;

namespace SugarTalk.Core.Domain.Meeting;

[Table("meeting_chat_room_setting")]
public class MeetingChatRoomSetting : IEntity
{
    public MeetingChatRoomSetting()
    {
        LastModifiedDate = DateTimeOffset.Now;
        SelfLanguage = SpeechTargetLanguageType.Cantonese;
        ListeningLanguage = SpeechTargetLanguageType.Cantonese;
    }
    
    [Key]
    [Column("id")]
    public int Id { get; set; }
    
    [Column("meeting_id", TypeName = "char(36)")]
    public Guid MeetingId { get; set; }

    [Column("user_id")] 
    public int UserId { get; set; }
    
    [Column("voice_name")]
    public string VoiceName { get; set; }
    
    [Column("voice_id")]
    public string VoiceId { get; set; }
    
    [Column("is_system", TypeName = "tinyint(1)")]
    public bool IsSystem { get; set; }
    
    [Column("transpose")]
    public float? Transpose { get; set; }
    
    [Column("speed")]
    public float? Speed { get; set; }
    
    [Column("style")]
    public int? Style { get; set; }
    
    [Column("inference_record_id")]
    public int? InferenceRecordId { get; set; }

    [Column("self_language")]
    public SpeechTargetLanguageType SelfLanguage { get; set; }
    
    [Column("listening_language")]
    public SpeechTargetLanguageType ListeningLanguage { get; set; }

    [Column("last_modified_date")]
    public DateTimeOffset LastModifiedDate { get; set; }
}