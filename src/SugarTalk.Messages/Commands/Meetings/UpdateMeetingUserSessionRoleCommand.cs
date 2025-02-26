using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class UpdateMeetingUserSessionRoleCommand : ICommand
{
    public Guid MeetingId { get; set; }

    public int UserId { get; set; }

    public MeetingUserSessionRole NewRole { get; set; }

    public bool? IsCoHost { get; set; }
}

public class UpdateMeetingUserSessionRoleResponse : SugarTalkResponse
{
}