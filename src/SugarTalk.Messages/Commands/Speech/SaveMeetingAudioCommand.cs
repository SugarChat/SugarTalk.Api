using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Enums.Speech;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Speech;

public class SaveMeetingAudioCommand : ICommand
{
    public Guid MeetingId { get; set; }
    
    public SpeechTargetLanguageType TranslatedLanguage { get; set; }
    
    public string AudioForBase64 { get; set; }
}

public class SaveMeetingAudioResponse : SugarTalkResponse<string>
{
}