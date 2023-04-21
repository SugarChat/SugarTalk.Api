using Mediator.Net.Contracts;
using SugarTalk.Messages.Dtos.Meetings;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Meetings;

public class GetMeetingByNumberRequest : IRequest
{
    public string MeetingNumber { get; set; }
}

public class GetMeetingByNumberResponse : SugarTalkResponse<MeetingDto>
{
}
