using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Meetings;


public class GetMeetingRecordCountRequest : IRequest
{
}

public class GetMeetingRecordCountResponse : SugarTalkResponse<int>
{
}