using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Attributes;
using SugarTalk.Messages.Enums.Speech;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings.Speak;

[AllowGuestAccess]
public class AddOrUpdateChatRoomSettingCommand : ICommand
{
    public Guid MeetingId { get; set; }
    
    public string VoiceId { get; set; }
    
    public bool IsSystem { get; set; }
    
    public string VoiceName { get; set; }
    
    public float? Speed { get; set; }
    
    public float? Transpose { get; set; }
    
    public int? Style { get; set; }
    
    public int? InferenceRecordId { get; set; }
    
    public SpeechTargetLanguageType SelfLanguage { get; set; }

    public SpeechTargetLanguageType ListeningLanguage { get; set; }
}

public class AddOrUpdateChatRoomSettingResponse : SugarTalkResponse
{
}