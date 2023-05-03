using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Ioc;
using SugarTalk.Messages.Dto.Users;

namespace SugarTalk.Core.Services.Account;

public interface IUserSessionDataProvider : IScopedDependency
{
    Task<List<UserSessionDto>> GetUserSessionsByMeetingId(Guid meetingId, CancellationToken cancellationToken);
        
    Task AddUserSessionAsync(UserSession userSession, CancellationToken cancellationToken);
        
    Task UpdateUserSessionAsync(UserSession userSession, CancellationToken cancellationToken);
}

public class UserSessionDataProvider : IUserSessionDataProvider
{
    private readonly IMapper _mapper;
    private readonly IRepository _repository;

    public UserSessionDataProvider(IMapper mapper, IRepository repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<List<UserSessionDto>> GetUserSessionsByMeetingId(Guid meetingId, CancellationToken cancellationToken)
    {
        var userSessions = await _repository.Query<UserSession>(x => x.MeetingId == meetingId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return _mapper.Map<List<UserSessionDto>>(userSessions);
    }

    public async Task AddUserSessionAsync(UserSession userSession, CancellationToken cancellationToken)
    {
        if (userSession != null)
            await _repository.InsertAsync(userSession, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateUserSessionAsync(UserSession userSession, CancellationToken cancellationToken)
    {
        if (userSession != null)
            await _repository.UpdateAsync(userSession, cancellationToken).ConfigureAwait(false);
    }
}
