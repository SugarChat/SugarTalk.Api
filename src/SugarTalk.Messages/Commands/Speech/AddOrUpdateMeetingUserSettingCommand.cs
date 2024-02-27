using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Attributes;
using SugarTalk.Messages.Enums.Speech;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Speech;

[AllowGuestAccess]
public class AddOrUpdateMeetingUserSettingCommand : ICommand
{
    public Guid MeetingId { get; set; }
    
    public SpeechTargetLanguageType TargetLanguageType { get; set; } 
}

public class AddOrUpdateMeetingUserSettingResponse : SugarTalkResponse
{
}