using System;
using SugarTalk.Messages.Enums.Speech;

namespace SugarTalk.Messages.Dto.Meetings.User;

public class MeetingUserSettingDto
{
    public Guid Id { get; set; }

    public int UserId { get; set; }

    public VoiceSamplesByLanguageType ListenedLanguageType { get; set; } 

    public SpeechTargetLanguageType TargetLanguageType { get; set; } 
}