using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class OutMeetingCommand : ICommand
{
    public Guid MeetingId { get; set; }
}

public class OutMeetingResponse : SugarTalkResponse<bool>
{
}