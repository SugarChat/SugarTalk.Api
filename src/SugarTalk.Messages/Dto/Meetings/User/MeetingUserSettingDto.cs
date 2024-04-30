using System;
using SugarTalk.Messages.Enums.Speech;

namespace SugarTalk.Messages.Dto.Meetings.User;

public class MeetingUserSettingDto
{
    public Guid Id { get; set; }

    public int UserId { get; set; }

    public SpanishToneType SpanishToneType { get; set; }
    
    public MandarinToneType MandarinToneType { get; set; }
    
    public EnglishToneType EnglishToneType{ get; set; }
    
    public KoreanToneType KoreanToneType { get; set; }
    
    public FrenchToneType FrenchToneType { get; set; }
    
    public CantoneseToneType CantoneseToneType { get; set; }

    public SpeechTargetLanguageType TargetLanguageType { get; set; } 
}