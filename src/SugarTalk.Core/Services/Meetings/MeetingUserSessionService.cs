using AutoMapper;
using SugarTalk.Core.Ioc;

namespace SugarTalk.Core.Services.Meetings;

public interface IMeetingUserSessionService : IScopedDependency
{
}

public class MeetingUserSessionService : IMeetingUserSessionService
{
    private readonly IMapper _mapper;
    private readonly IMeetingUserSessionDataProvider _meetingUserSessionDataProvider;

    public MeetingUserSessionService(IMapper mapper, IMeetingUserSessionDataProvider meetingUserSessionDataProvider)
    {
        _mapper = mapper;
        _meetingUserSessionDataProvider = meetingUserSessionDataProvider;
    }
}