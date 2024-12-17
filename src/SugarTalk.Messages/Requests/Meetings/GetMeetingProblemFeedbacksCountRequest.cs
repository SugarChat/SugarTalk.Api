using Mediator.Net.Contracts;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.Meetings;

public class GetMeetingProblemFeedbacksCountRequest : IRequest
{
}

public class GetMeetingProblemFeedbacksCountResponse : SugarTalkResponse<int>
{
}
