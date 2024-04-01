using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class UpdateInProcessMeetingCommand : ICommand
{
    public Guid Id { get; set; }

    public bool IsEa { get; set; }
}

public class UpdateInProcessMeetingResponse : SugarTalkResponse
{
}