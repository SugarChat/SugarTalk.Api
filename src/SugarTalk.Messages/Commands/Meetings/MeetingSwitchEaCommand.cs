using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class MeetingSwitchEaCommand : ICommand
{
    public Guid Id { get; set; }

    public bool IsActiveEa { get; set; }
}

public class MeetingSwitchEaResponse : SugarTalkResponse
{
}