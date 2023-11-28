using Mediator.Net.Contracts;
using SugarTalk.Messages.Enums.Speech;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Speech;

public class AddOrUpdateMeetingUserSettingCommand : ICommand
{
    public VoiceSamplesByLanguageType ListenedLanguageType { get; set; }

    public SpeechTargetLanguageType TargetLanguageType { get; set; } 
}

public class AddOrUpdateMeetingUserSettingResponse : SugarTalkResponse
{
}