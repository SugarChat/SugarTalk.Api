using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.MeetingMonitor;
using SugarTalk.Core.Ioc;

namespace SugarTalk.Core.Services.MeetingMonitor;

public interface IMeetingMonitorDataProvider : IScopedDependency
{
    Task<MeetingMonitorKnowledge> GetMeetingMonitorKnowledgeAsync(CancellationToken cancellationToken);
}

public class MeetingMonitorDataProvider : IMeetingMonitorDataProvider
{
    private readonly IMapper _mapper;
    private readonly IRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public MeetingMonitorDataProvider(IMapper mapper, IRepository repository, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MeetingMonitorKnowledge> GetMeetingMonitorKnowledgeAsync(CancellationToken cancellationToken)
    {
        return await _repository.Query<MeetingMonitorKnowledge>().FirstOrDefaultAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}