using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Enums.Speech;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings.Speak;

public class AddOrUpdateChatRoomSettingCommand : ICommand
{
    public Guid MeetingId { get; set; }
    
    public string VoiceId { get; set; }
    
    public SpeechTargetLanguageType SelfLanguage { get; set; }
    
    public SpeechTargetLanguageType ListeningLanguage { get; set; }
}

public class AddOrUpdateChatRoomSettingResponse : SugarTalkResponse
{
}