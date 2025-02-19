using System;
using System.Collections.Generic;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Meetings;

public class GetAllMeetingUserSessionsForMeetingIdRequest : IRequest
{
    public Guid MeetingId { get; set; }
}

public class GetAllMeetingUserSessionsForMeetingIdResponse : SugarTalkResponse<GetAllMeetingUserSessionsForMeetingIdDto>
{
}

public class GetAllMeetingUserSessionsForMeetingIdDto
{
    public List<MeetingUserSessionDto> MeetingUserSessions { get; set; }

    public int Count { get; set; }
}