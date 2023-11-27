using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;
using SugarTalk.Messages.Enums.Speech;

namespace SugarTalk.Messages.Commands.Speech;

public class SaveMeetingAudioCommand : ICommand
{
    public SaveMeetingAudioCommand()
    {
        TargetLanguageType = SpeechTargetLanguageType.Cantonese;
    }

    public Guid MeetingId { get; set; }
    
    public string AudioForBase64 { get; set; }

    public SpeechTargetLanguageType TargetLanguageType { get; set; } 
}

public class SaveMeetingAudioResponse : SugarTalkResponse<string>
{
}