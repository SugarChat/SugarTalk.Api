using System;
using System.Collections.Generic;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Commands.Meetings;

public class CreateMeetingInvitationRecordsCommand : ICommand
{
    public Guid MeetingId { get; set; }

    public Guid? MeetingSubId { get; set; }

    public List<string> Names { get; set; }
}

public class CreateMeetingInvitationRecordsResponse : SugarTalkResponse<MeetingInvitationRecordDto>
{
}