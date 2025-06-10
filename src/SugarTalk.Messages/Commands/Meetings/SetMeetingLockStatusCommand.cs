using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class SetMeetingLockStatusCommand : ICommand
{
    public Guid MeetingId { get; set; }
    
    public bool IsLocked { get; set; }
}

public class SetMeetingLockStatusResponse : SugarTalkResponse<bool>
{
}