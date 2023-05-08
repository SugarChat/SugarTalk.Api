using AutoMapper;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Services.Account;

namespace SugarTalk.Core.Services.Meetings;

public interface IUserSessionService : IScopedDependency
{
}

public class UserSessionService : IUserSessionService
{
    private readonly IMapper _mapper;
    private readonly IUserSessionDataProvider _userSessionDataProvider;

    public UserSessionService(IMapper mapper, IUserSessionDataProvider userSessionDataProvider)
    {
        _mapper = mapper;
        _userSessionDataProvider = userSessionDataProvider;
    }
}