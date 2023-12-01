using System;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings.User;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Meetings.User;

public class GetMeetingUserSettingRequest : IRequest
{
    public Guid MeetingId { get; set;}
}

public class GetMeetingUserSettingResponse : SugarTalkResponse<MeetingUserSettingDto>
{
}