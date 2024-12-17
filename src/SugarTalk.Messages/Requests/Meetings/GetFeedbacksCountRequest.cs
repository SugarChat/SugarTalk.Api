using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Meetings;

public class GetFeedbacksCountRequest : IRequest
{
}

public class GetFeedbacksCountResponse : SugarTalkResponse<int>
{
}
