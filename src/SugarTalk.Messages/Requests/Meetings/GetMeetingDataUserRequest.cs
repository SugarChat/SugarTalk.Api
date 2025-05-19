using System;
using System.Collections.Generic;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Meetings;

public class GetMeetingDataUserRequest : IRequest
{
    public DateTimeOffset? Day { get; set; }
}

public class GetMeetingDataUserResponse : SugarTalkResponse<List<GetMeetingDataUserDto>>
{
}

public class GetMeetingDataUserDto
{
    public Guid MeetingId { get; set; }

    public string FundationId { get; set; }

    public string UserName { get; set; }

    public long MeetingStartTime { get; set; }

    public DateTimeOffset Date { get; set; }
}