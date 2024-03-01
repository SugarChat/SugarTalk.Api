using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Attributes;
using SugarTalk.Messages.Enums.Speech;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Speech;

[AllowGuestAccess]
public class UpdateMeetingSpeechCommand : ICommand
{
    public Guid MeetingSpeechId { get; set; }
    
    public SpeechStatus Status { get; set; }
}

public class UpdateMeetingSpeechResponse : SugarTalkResponse<string>
{
}