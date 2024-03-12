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
        VoiceType = ChatRoomVoiceType.XiaochenNeural;
    }
    
    [Key]
    [Column("id")]
    public int Id { get; set; }
    
    [Column("meeting_id", TypeName = "char(36)")]
    public Guid MeetingId { get; set; }

    [Column("user_id")] 
    public int UserId { get; set; }
    
    [Column("ea_voice_id", TypeName = "char(36)")]
    public Guid EaVoiceId { get; set; }
    
    [Column("original_language_type")]
    public SpeechTargetLanguageType SelfLanguage { get; set; }
    
    [Column("target_language_type")]
    public SpeechTargetLanguageType ListeningLanguage { get; set; }
    
    [Column("voice_type")]
    public ChatRoomVoiceType VoiceType { get; set; }
    
    [Column("last_modified_date")]
    public DateTimeOffset LastModifiedDate { get; set; }
}