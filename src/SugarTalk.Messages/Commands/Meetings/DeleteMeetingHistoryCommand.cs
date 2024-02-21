using System;
using System.Collections.Generic;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class DeleteMeetingHistoryCommand : ICommand
{
    public List<Guid> MeetingHistoryIds { get; set; }
}

public class DeleteMeetingHistoryResponse : SugarTalkResponse
{
}
