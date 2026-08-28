using System;
using System.Collections.Generic;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Attributes;
using SugarTalk.Messages.Constants;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

[SugarTalkAuthorize(new string[] { }, new[] { SecurityStore.Permissions.CanGetMeetingRelatedData })]
public class DeleteMeetingHistoryCommand : ICommand
{
    public List<Guid> MeetingHistoryIds { get; set; }
}

public class DeleteMeetingHistoryResponse : SugarTalkResponse
{
}
