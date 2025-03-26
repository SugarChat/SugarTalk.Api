using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Meetings;

public class GetMeetingRecordEgressRequest : IRequest
{
    public Guid MeetingId { get; set; }
}

public class GetMeetingRecordEgressResponse : SugarTalkResponse<string>
{
}