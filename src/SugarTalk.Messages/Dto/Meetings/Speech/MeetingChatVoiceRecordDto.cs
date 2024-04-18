using System;
using SugarTalk.Messages.Enums.Speech;

namespace SugarTalk.Messages.Dto.Meetings.Speech;

public class MeetingChatVoiceRecordDto
{
    public Guid Id { get; set; }
    
    public SpeechTargetLanguageType VoiceLanguage { get; set; }
    
    public DateTimeOffset CreatedDate { get; set; }
    
    public Guid SpeechId { get; set; }
    
    public string VoiceId { get; set; }
    
    public bool IsSystemVoice { get; set; }
    
    public string VoiceUrl { get; set; }
    
    public bool IsSelf { get; set; }
    
    public string TranslatedText { get; set; }
    
    public string InferenceRecordId { get; set; }

    public ChatRecordGenerationStatus GenerationStatus { get; set; }
}
