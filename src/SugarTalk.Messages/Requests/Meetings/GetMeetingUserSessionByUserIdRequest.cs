using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.Meetings;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Meetings;

public class GetMeetingUserSessionByUserIdRequest : IRequest
{
    public int UserId { get; set; }
}

public class GetMeetingUserSessionByUserIdResponse : SugarTalkResponse<MeetingUserSessionDto>
{
}