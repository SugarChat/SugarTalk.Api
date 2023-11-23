using System.Collections.Generic;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Meetings;

public class GetMeetingUserSessionsRequest : IRequest
{
    public List<int> Ids { get; set; }
}

public class GetMeetingUserSessionsResponse : SugarTalkResponse<List<MeetingUserSessionDto>>
{
}
