using System;
using System.Collections.Generic;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class DeleteMeetingRecordCommand : ICommand
{
    public List<Guid> MeetingRecordIds { get; set; }
}

public class DeleteMeetingRecordResponse : SugarTalkResponse
{
}
