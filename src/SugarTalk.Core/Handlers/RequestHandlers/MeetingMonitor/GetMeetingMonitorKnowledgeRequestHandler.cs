using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Account;
using SugarTalk.Core.Services.MeetingMonitor;
using SugarTalk.Messages.Requests.MeetingMonitor;

namespace SugarTalk.Core.Handlers.RequestHandlers.MeetingMonitor;

public class GetMeetingMonitorKnowledgeRequestHandler : IRequestHandler<GetMeetingMonitorKnowledgeRequest, GetMeetingMonitorKnowledgeResponse>
{
    private readonly IMeetingMonitorService  _meetingMonitorService;

    public GetMeetingMonitorKnowledgeRequestHandler(IMeetingMonitorService meetingMonitorService)
    {
        _meetingMonitorService = meetingMonitorService;
    }

    public async Task<GetMeetingMonitorKnowledgeResponse> Handle(IReceiveContext<GetMeetingMonitorKnowledgeRequest> context, CancellationToken cancellationToken)
    {
        return await _meetingMonitorService.GetMeetingMonitorKnowledgeAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}