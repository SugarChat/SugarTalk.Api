using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Attributes;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Speech;

[AllowGuestAccess]
public class SaveMeetingAudioCommand : ICommand
{
    public Guid MeetingId { get; set; }
    
    public string AudioForBase64 { get; set; }
}

public class SaveMeetingAudioResponse : SugarTalkResponse<string>
{
}