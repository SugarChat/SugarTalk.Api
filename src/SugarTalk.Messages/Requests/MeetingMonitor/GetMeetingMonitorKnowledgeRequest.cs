using Mediator.Net.Contracts;
using SugarTalk.Messages.Dto.MeetingMonitor;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Messages.Requests.MeetingMonitor;

public class GetMeetingMonitorKnowledgeRequest : IRequest
{
}

public class GetMeetingMonitorKnowledgeResponse : SugarTalkResponse<GetMeetingMonitorKnowledgeResponseData>
{
}

public class GetMeetingMonitorKnowledgeResponseData
{
    public MeetingMonitorKnowledgeDto Knowledge { get; set; }
}