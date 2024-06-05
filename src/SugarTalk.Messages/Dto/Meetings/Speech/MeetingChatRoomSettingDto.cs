using System;
using SugarTalk.Messages.Enums.Speech;

namespace SugarTalk.Messages.Dto.Meetings.Speech;

public class MeetingChatRoomSettingDto
{
    public int Id { get; set; }
    
    public Guid MeetingId { get; set; }

    public int UserId { get; set; }
    
    public string VoiceName { get; set; }
    
    public string VoiceId { get; set; }
    
    public bool IsSystem { get; set; }
    
    public float? Transpose { get; set; }
    
    public float? Speed { get; set; }
    
    public int? Style { get; set; }
    
    public int? InferenceRecordId { get; set; }

    public SpeechTargetLanguageType SelfLanguage { get; set; }
    
    public SpeechTargetLanguageType ListeningLanguage { get; set; }

    public DateTimeOffset LastModifiedDate { get; set; }
}