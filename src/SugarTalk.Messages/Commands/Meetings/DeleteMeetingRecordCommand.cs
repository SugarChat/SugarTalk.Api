using System;
using System.Collections.Generic;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Attributes;
using SugarTalk.Messages.Constants;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

[SugarTalkAuthorize(new string[] { }, new[] { SecurityStore.Permissions.CanGetMeetingRelatedData })]
public class DeleteMeetingRecordCommand : ICommand
{
    public List<Guid> MeetingRecordIds { get; set; }
}

public class DeleteMeetingRecordResponse : SugarTalkResponse
{
}
