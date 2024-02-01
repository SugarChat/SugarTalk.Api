using SugarTalk.Core.Ioc;
using SugarTalk.Core.Data;
using SugarTalk.Core.Services.Utils;

namespace SugarTalk.Core.Services.Meetings;

public interface IMeetingProcessJobService : IScopedDependency
{
}

public class MeetingProcessJobService : IMeetingProcessJobService
{
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMeetingDataProvider _meetingDataProvider;

    public MeetingProcessJobService(IClock clock, IUnitOfWork unitOfWork, IMeetingDataProvider meetingDataProvider)
    {
        _clock = clock;
        _unitOfWork = unitOfWork;
        _meetingDataProvider = meetingDataProvider;
    }
}