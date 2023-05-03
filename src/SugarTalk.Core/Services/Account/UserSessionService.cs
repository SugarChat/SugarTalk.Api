using AutoMapper;
using SugarTalk.Core.Ioc;

namespace SugarTalk.Core.Services.Account;

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