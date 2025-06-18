using System.Collections.Generic;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Enums.Meeting;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class UpdateMeetingUserSessionTypeCommand : ICommand
{
    public List<int> Ids { get; set; }

    public MeetingUserSessionOnlineType? OnlineType { get; set; }

    public bool? AllowEntryMeeting { get; set; }
}

public class UpdateMeetingUserSessionTypeResponse : SugarTalkResponse<List<MeetingUserSessionDto>>
{
}