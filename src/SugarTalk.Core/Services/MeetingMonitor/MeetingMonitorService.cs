using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using SugarTalk.Core.Ioc;
using SugarTalk.Messages.Dto.MeetingMonitor;
using SugarTalk.Messages.Requests.MeetingMonitor;

namespace SugarTalk.Core.Services.MeetingMonitor;

public interface IMeetingMonitorService : IScopedDependency
{        
    Task<GetMeetingMonitorKnowledgeResponse> GetMeetingMonitorKnowledgeAsync(GetMeetingMonitorKnowledgeRequest request, CancellationToken cancellationToken);
    
}

public class MeetingMonitorService : IMeetingMonitorService
{
    private readonly IMapper _mapper;
    private readonly IMeetingMonitorDataProvider _meetingMonitorDataProvider;
    
    public MeetingMonitorService(IMapper mapper, IMeetingMonitorDataProvider meetingMonitorDataProvider)
    {
        _mapper = mapper;
        _meetingMonitorDataProvider = meetingMonitorDataProvider;
    }

    public async Task<GetMeetingMonitorKnowledgeResponse> GetMeetingMonitorKnowledgeAsync(GetMeetingMonitorKnowledgeRequest request, CancellationToken cancellationToken)
    {
        var konwledge = await _meetingMonitorDataProvider
            .GetMeetingMonitorKnowledgeAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return new GetMeetingMonitorKnowledgeResponse()
        {
            Data = new GetMeetingMonitorKnowledgeResponseData()
            {
                Knowledge = _mapper.Map<MeetingMonitorKnowledgeDto>(konwledge)
            }
        };
    }
}